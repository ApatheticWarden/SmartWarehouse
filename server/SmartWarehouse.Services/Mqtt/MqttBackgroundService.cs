using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using System.Text;

namespace SmartWarehouse.Services.Mqtt {
    public class MqttBackgroundService : BackgroundService {
        private readonly ILogger<MqttBackgroundService> _logger;
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttClientOptions;
        private readonly MqttClientFactory _mqttFactory;

        // No hardcode, just chill
        private readonly IConfiguration _config;

        public MqttBackgroundService(ILogger<MqttBackgroundService> logger, IConfiguration config) {
            _logger = logger;
            _config = config;

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

        private Task HandleIncomingMessage(MqttApplicationMessageReceivedEventArgs e) {
            string topic = e.ApplicationMessage.Topic;
            string payload = e.ApplicationMessage.ConvertPayloadToString();

            _logger.LogInformation($"[MQTT RAW] Got from topic {topic}: {payload}");

            // Some actions here

            return Task.CompletedTask;
        }

        // Executed on start 
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
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
    }
}