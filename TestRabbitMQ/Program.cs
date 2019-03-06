using EventBus.Abstractions;
using EventBus.RabbitMQ;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using YxRadio.Common.Logger.RabbitMQSettings;

namespace TestRabbitMQ
{
    class Program
    {
        static void Main(string[] args)
        {
            IRabbitMQClient client = new RabbitMQClient(RabbitMQConfiguration.Default);
            //client.Publish(new object());

            client.Handle<object>("peatio.events.ranger", "#", x =>
            {
                Console.WriteLine(JsonConvert.SerializeObject(x));
            });
            //RattitSimpleConsumer();
            Console.ReadKey();
        }

        public static void RattitSimpleConsumer()
        {
            var factory = new ConnectionFactory() { HostName = "213.136.82.97" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "peatio.events.ranger", type: "topic");

                channel.QueueBind(queue: ".ranger.d7476361baae7142",
                                    exchange: "peatio.events.ranger",
                                    routingKey: "#	");

                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received '{0}'", message);
                };
                channel.BasicConsume(queue: ".ranger.d7476361baae7142",
                                     autoAck: true,
                                     consumer: consumer);
            }
        }
    }

    [JsonObject(NamingStrategyType = typeof(DefaultNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    public class OrderBookMessage : MessageBase
    {
        public OrderBookMessage()
        {
        }

        public string Product { get; set; }
        public Dictionary<decimal, decimal> Bids { get; set; }
        public Dictionary<decimal, decimal> Asks { get; set; }
    }

    public abstract class MessageBase : IEvent, INotification
    {
        public MessageBase()
        {
            CreationDate = DateTime.UtcNow;
        }
        public string Type { get; set; }
        public DateTime CreationDate { get; }
    }
}
