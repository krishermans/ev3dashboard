
using System;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;

namespace ev3dashboard.frontend.Data
{
    public class EV3BrokerService : IDisposable
    {
        private const string MqttBrokerHostname = "192.168.0.30";
        private const int MqttBrokerPort = 1883;
        private readonly string ClientId = $"EV3DashboardClient_{Guid.NewGuid().ToString()}";
        private const string Topic = "Eleven";

        private readonly IManagedMqttClient _mqttClient;

        public EV3BrokerService()
        {
            var mqttFactory = new MqttFactory();
            _mqttClient = mqttFactory.CreateManagedMqttClient();
            SetupMqttClient(_mqttClient).Wait();

        }

        public string ConnectionStatus { get; set; }

        public async Task Send(string message)
        {
            await _mqttClient.PublishAsync(CreateMessage(message));
        }

        private MqttApplicationMessage CreateMessage(string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(Topic)
                .WithPayload(payload)
                .WithAtLeastOnceQoS()
                .WithRetainFlag()
                .Build();
            
            return message;
        }

        private async Task SetupMqttClient(IManagedMqttClient mqttClient)
        {
            await mqttClient.SubscribeAsync(CreateTopicFilter());

            mqttClient.UseConnectedHandler(e =>
            {
                ConnectionStatus = $"### CONNECTED WITH SERVER {MqttBrokerHostname} ###";
            });

            mqttClient.UseDisconnectedHandler(e =>
            {
                ConnectionStatus = $"### DISCONNECTED FROM SERVER: {e.Exception.GetBaseException().Message} ###";
            });

            var options = CreateManagedMqttClientOptions();
            await mqttClient.StartAsync(options);
        }

        private  MqttTopicFilter CreateTopicFilter()
        {
            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(Topic)
                .WithAtLeastOnceQoS()
                .Build();
            
            return topicFilter;
        }

        private  ManagedMqttClientOptions CreateManagedMqttClientOptions()
        {
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId(ClientId)
                    .WithTcpServer(MqttBrokerHostname, MqttBrokerPort)
                    //.WithCredentials(Username, Password)
                    //.WithTls(UpdateTlsParameters)
                    .Build())
                .Build();
            return options;
        }

        public void Dispose()
        {
            _mqttClient?.StopAsync().Wait(TimeSpan.FromSeconds(5));
        }
    }
}
