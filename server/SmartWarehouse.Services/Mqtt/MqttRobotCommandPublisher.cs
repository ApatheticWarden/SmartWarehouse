using Microsoft.Extensions.Configuration;
using MQTTnet;
using SmartWarehouse.Models.DTOs;
using SmartWarehouse.Services.Interfaces;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SmartWarehouse.Services.Mqtt {
    public class MqttRobotCommandPublisher : IRobotCommandPublisher {
        private readonly IConfiguration _config;

        public MqttRobotCommandPublisher(IConfiguration config) {
            _config = config;
        }

        private static readonly JsonSerializerOptions _jsonOptions = new() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task PublishTaskCommandAsync(int robotId, RobotTaskCommandDTO command) {
            // get conf
            string brokerIp = _config["MqttSettings:BrokerIp"] ?? throw new InvalidOperationException("BrokerIp absent in appsettings");
            int port = _config.GetValue<int?>("MqttSettings:Port") ?? throw new InvalidOperationException("Port absent in appsettings"); // try to get val, if null -> set 1883
            string clientId = _config["MqttSettings:ClientId"] ?? throw new InvalidOperationException("ClientId absent in appsettings");

            var mqttFactory = new MqttClientFactory();
            using var mqttClient = mqttFactory.CreateMqttClient();
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(brokerIp, port)
                .WithClientId(clientId + "-publisher")
                .Build();

            await mqttClient.ConnectAsync(mqttClientOptions);

            var topic = $"smartwh/robots/{robotId}/task/command";
            var payload = JsonSerializer.Serialize(command, _jsonOptions);

            var appMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();

            await mqttClient.PublishAsync(appMessage);
            await mqttClient.DisconnectAsync();
        }
    }
}
