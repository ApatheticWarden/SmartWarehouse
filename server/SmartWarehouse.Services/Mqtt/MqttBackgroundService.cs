using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using SmartWarehouse.Models.DTOs;
using SmartWarehouse.Services.Interfaces;
using System.Text.Json;
using System.Threading.Channels;

namespace SmartWarehouse.Services.Mqtt {
    public class MqttBackgroundService : BackgroundService {
        private readonly ILogger<MqttBackgroundService> _logger;
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttClientOptions;
        private readonly MqttClientFactory _mqttFactory;

        // For scoped ones
        private readonly IServiceScopeFactory _scopeFactory;

        // No hardcode, just chill
        private readonly IConfiguration _config;


        static readonly JsonSerializerOptions _jsonOptions = new() {
            PropertyNameCaseInsensitive = true
        };

        // Channels
        // 1000 | 5000 - param which defines how many messages would be there
        private readonly Channel<MqttApplicationMessage> _taskChannel =
            Channel.CreateBounded<MqttApplicationMessage>(new BoundedChannelOptions(1000) {
                FullMode = BoundedChannelFullMode.Wait
            });

        private readonly Channel<MqttApplicationMessage> _telemetryChannel =
            Channel.CreateBounded<MqttApplicationMessage>(new BoundedChannelOptions(5000) {
                FullMode = BoundedChannelFullMode.DropOldest
            });

        public MqttBackgroundService(ILogger<MqttBackgroundService> logger, 
            IConfiguration config,
            IServiceScopeFactory scopeFactory) {
            _logger = logger;
            _config = config;
            _scopeFactory = scopeFactory;

            // get conf
            string brokerIp = _config["MqttSettings:BrokerIp"] ?? throw new InvalidOperationException("BrokerIp absent in appsettings");
            int port = _config.GetValue<int?>("MqttSettings:Port") ?? throw new InvalidOperationException("Port absent in appsettings"); // try to get val, if null -> set 1883
            string clientId = _config["MqttSettings:ClientId"] ?? throw new InvalidOperationException("ClientId absent in appsettings");

            // Creating an client
            _mqttFactory = new MqttClientFactory();
            _mqttClient = _mqttFactory.CreateMqttClient();

            // Apply settings
            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(brokerIp, port)
                .WithClientId(clientId)
                .WithCleanSession() // Cleans all topics on disconnect
                .Build();
            _mqttClient.ApplicationMessageReceivedAsync += HandleIncomingMessage;

            // Handle disconnect
            _mqttClient.DisconnectedAsync += e => 
            {
                _logger.LogWarning($"[MQTT] Lost connection with Broker. Reason: {e.Reason}");
                return Task.CompletedTask;
            };
        }

        // This func must be "dumb", no hard calcs here
        private async Task HandleIncomingMessage(MqttApplicationMessageReceivedEventArgs e) {
            string topic = e.ApplicationMessage.Topic;
            try {
                if (topic.EndsWith("task/update", StringComparison.Ordinal)) {
                    await _taskChannel.Writer.WriteAsync(e.ApplicationMessage);
                } else if (topic.EndsWith("telemetry", StringComparison.Ordinal)) {
                    _telemetryChannel.Writer.TryWrite(e.ApplicationMessage);
                } else {
                    _logger.LogWarning($"[MQTT RAW] Unhandled topic routed: {e.ApplicationMessage.Topic}");
                }
            } catch (Exception ex) {
                _logger.LogError(ex, $"[MQTT ERROR] Failed to process message from topic {e.ApplicationMessage.Topic}");
            }
        }

        private async Task ManageMqttConnectionAsync(CancellationToken stoppingToken) {
            var subscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic("smartwh/robots/+/telemetry"))
                .WithTopicFilter(f => f.WithTopic("smartwh/robots/+/task/update"))
                .Build();

            while (!stoppingToken.IsCancellationRequested) {
                if (!_mqttClient.IsConnected) {
                    try {
                        // Try to recon
                        _logger.LogInformation("[MQTT] Trying to reconnect");
                        await _mqttClient.ConnectAsync(_mqttClientOptions, stoppingToken);
                        _logger.LogInformation("[MQTT] Successfuly reconnected");

                        await _mqttClient.SubscribeAsync(subscribeOptions, stoppingToken);
                        _logger.LogInformation("[MQTT] Re-Subscribed topics");
                    } catch (Exception ex) {
                        _logger.LogError($"[MQTT Error] Connection failed: {ex.Message}");
                    }
                }

                // little delay
                await Task.Delay(1000, stoppingToken);
            }

            if (_mqttClient.IsConnected) {
                // Set disconnect options to let the server know
                var disconnectOptions = _mqttFactory.CreateClientDisconnectOptionsBuilder()
                    .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
                    .Build();
                await _mqttClient.DisconnectAsync(disconnectOptions, CancellationToken.None);
                _logger.LogInformation("[MQTT] Client stopped normally");
            }
        }

        private async Task ProcessTasksAsync(CancellationToken stoppingToken) {
            try {
                await foreach (var message in _taskChannel.Reader.ReadAllAsync(stoppingToken)) {
                    try {
                        string payload = message.ConvertPayloadToString();
                        _logger.LogInformation($"[TASK WORKER] Processing: {payload}");

                        // Here do deserialization
                        var taskDto = JsonSerializer.Deserialize<TaskAnswerDTO>(payload, _jsonOptions);

                        if (taskDto == null) {
                            _logger.LogWarning("[TASK WORKER] Task answer is null");
                            continue;
                        }

                        // Get needy service 
                        using var scope = _scopeFactory.CreateScope();

                        // e.x. var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

                        // Some actions here
                        // e.x. await dbContext.SaveChangesAsync(stoppingToken);

                        var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

                        // - if completed
                        if (taskDto.Status.Equals(Models.Enums.RobotTaskStatus.Completed)) {
                            var res = await taskService.HandleTaskCompletedAsync(taskDto.TaskId);
                            if (!res.IsSuccess) _logger.LogError("[TASK WORKER] Failed to handle task completion for [{TaskId}]. Reason: {Reason}",
                                taskDto.TaskId,
                                res.Message
                                );
                        }
                        // - if started
                        else if (taskDto.Status.Equals(Models.Enums.RobotTaskStatus.Started)) {
                            var res = await taskService.HandleTaskStartedAsync(taskDto.TaskId);
                            if (!res.IsSuccess) _logger.LogError("[TASK WORKER] Failed to handle task start for [{TaskId}]. Reason: {Reason}",
                                taskDto.TaskId, 
                                res.Message
                                );
                        } 
                        // - if accepted
                        else if (taskDto.Status.Equals(Models.Enums.RobotTaskStatus.Accepted)) {
                            var res = await taskService.HandleTaskAcceptedAsync(taskDto.TaskId);
                            if (!res.IsSuccess) _logger.LogError("[TASK WORKER] Failed to handle task acceptation for [{TaskId}]. Reason: {Reason}",
                                taskDto.TaskId,
                                res.Message
                                );
                        }
                        // - if failed
                        else if (taskDto.Status.Equals(Models.Enums.RobotTaskStatus.Failed)) {
                            var res = await taskService.HandleTaskFailedAsync(taskDto.TaskId, taskDto.Message);
                            if (!res.IsSuccess) _logger.LogError(
                                "[TASK WORKER] Failed to handle task failure for task [{TaskId}]. Reason: {Reason}",
                                taskDto.TaskId,
                                res.Message
                            );
                        } else {
                            _logger.LogError("[TASK WORKER] Unsupported status passed {TaskStatus}", taskDto.Status);
                        }
                    } catch (Exception ex) {
                        _logger.LogError(ex, "[TASK WORKER] Failed to process message");
                    }
                }
            } catch (OperationCanceledException) {
                _logger.LogInformation("[TASK WORKER] Stopped gracefully");
            } catch (Exception ex) {
                _logger.LogCritical(ex, "[TASK WORKER] Fatal error, channel reading stopped!");
            }
        }

        private async Task ProcessTelemetryAsync(CancellationToken stoppingToken) {
            try {
                await foreach (var message in _telemetryChannel.Reader.ReadAllAsync(stoppingToken)) {
                    try {
                        string payload = message.ConvertPayloadToString();
                        _logger.LogInformation($"[TELEMETRY WORKER] Processing: {payload}");

                        var telemetryDTO = JsonSerializer.Deserialize<RobotTelemetryDTO>(payload, _jsonOptions);

                        if (telemetryDTO == null) {
                            _logger.LogWarning("[TELEMETRY WORKER] Telemetry answer is null");
                            continue;
                        }

                        using var scope = _scopeFactory.CreateScope();
                        var telemetryService = scope.ServiceProvider.GetRequiredService<ITelemetryService>();

                        var res = telemetryService.HandleRobotTelemetry(telemetryDTO);
                        if (!res.IsSuccess) _logger.LogWarning(res.Message);

                    } catch (Exception ex) {
                        _logger.LogError(ex, "[TELEMETRY WORKER] Failed to process message");
                    }
                }
            } catch (OperationCanceledException) {
                _logger.LogInformation("[TELEMETRY WORKER] Stopped gracefully");
            } catch (Exception ex) {
                _logger.LogCritical(ex, "[TELEMETRY WORKER] Fatal error, channel reading stopped!");
            }
        }

        // Executed on start 
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            // Our tasks
            var taskProcessingTask = ProcessTasksAsync(stoppingToken);
            var telemetryProcessingTask = ProcessTelemetryAsync(stoppingToken);
            // Main task
            var mqttConnectionTask = ManageMqttConnectionAsync(stoppingToken);

            // Wait its simultaneously work
            await Task.WhenAll(mqttConnectionTask, telemetryProcessingTask, taskProcessingTask);
        }
    }
}