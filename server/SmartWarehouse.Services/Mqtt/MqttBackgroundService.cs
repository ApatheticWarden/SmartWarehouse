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

        // No hardcode, just chill
        private readonly IConfiguration _config;

        public MqttBackgroundService(ILogger<MqttBackgroundService> logger, IConfiguration config) {
            _logger = logger;
            _config = config;

            // get conf
            string brockerIp = _config["MqqtSettings:BrokerIp"];
            int port = int.Parse(_config["MqttSettings:Port"]);
            string clientId = _config["MqttSettings:ClientId"];

            // Creating an client
            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            // Apply settings
            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(brockerIp, port)
                .WithClientId(clientId)
                .WithCleanSession().Build();
            _mqttClient.ApplicationMessageReceivedAsync += HandleIncomingMessage;
        }

        private Task HandleIncomingMessage(MqttApplicationMessageReceivedEventArgs e) {
            string topic = e.ApplicationMessage.Topic;
            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            _logger.LogInformation($"[MQTT RAW] Got from topic {topic}: {payload}");

            // Some actions here

            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            // code here

            while (!stoppingToken.IsCancellationRequested) {
                // little delay
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
