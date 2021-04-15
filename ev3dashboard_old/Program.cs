using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;

namespace ev3dashboard
{
    class Program
    {
        private const string MqttBrokerHostname = "192.168.0.30";
        private const int MqttBrokerPort = 1883;
        private static readonly string ClientId = $"Client_{Guid.NewGuid().ToString()}";
        private const string Topic = "Eleven";
        private const string Message = "Led Left On";
        static void Main(string[] args)
        {
            var mqttClient = new MqttFactory().CreateManagedMqttClient();
            SetupMqttClient(mqttClient).Wait();
            
            Console.WriteLine("Hello World!");

            Thread.Sleep(TimeSpan.FromMinutes(2));

            mqttClient.StopAsync().Wait(TimeSpan.FromSeconds(5));
        }

        private static async Task SetupMqttClient(IManagedMqttClient mqttClient)
        {
            await mqttClient.SubscribeAsync(CreateTopicFilter());

            var options = CreateManagedMqttClientOptions();
            mqttClient.UseApplicationMessageReceivedHandler(async e =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retained = {e.ApplicationMessage.Retain}");
                Console.WriteLine($"+ ProcessingFailed = {e.ProcessingFailed}");
                Console.WriteLine();
            });

            mqttClient.UseConnectedHandler(e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");
            });

            mqttClient.UseDisconnectedHandler(e =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                Console.WriteLine(e.Exception.GetBaseException().Message);
            });

            await mqttClient.StartAsync(options);

            //await mqttClient.PublishAsync(CreateMessage());
        }

        private static ManagedMqttClientOptions CreateManagedMqttClientOptions()
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

        private static MqttTopicFilter CreateTopicFilter()
        {
            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(Topic)
                .WithAtLeastOnceQoS()
                .Build();
            
            return topicFilter;
        }

        private static MqttApplicationMessage CreateMessage()
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(Topic)
                .WithPayload(Message)
                .WithAtLeastOnceQoS()
                .WithRetainFlag()
                .Build();
            
            return message;
        }
    }
}
