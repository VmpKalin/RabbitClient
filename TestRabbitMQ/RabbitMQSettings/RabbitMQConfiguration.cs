using RabbitMQ.Client;
using System;

namespace YxRadio.Common.Logger.RabbitMQSettings
{
    public class RabbitMQConfiguration
    {
        public string Hostname = "213.136.82.97";
        public string Username = ConnectionFactory.DefaultUser;
        public string Password = ConnectionFactory.DefaultPass;
        public string Exchange = string.Empty;
        public string ExchangeType = string.Empty;
        public RabbitMQDeliveryMode DeliveryMode = RabbitMQDeliveryMode.NonDurable;
        public string RouteKey = "#";
        public int Port = 5672;
        public string VHost = ConnectionFactory.DefaultVHost;
        public IProtocol Protocol = Protocols.DefaultProtocol;
        public ushort Heartbeat;
        public int BatchPostingLimit;
        public TimeSpan Period;

        public RabbitMQConfiguration(string routeKey, int batchPostingLimit, TimeSpan period)
        {
            RouteKey = routeKey;
            BatchPostingLimit = batchPostingLimit;
            Period = period;
        }

        public RabbitMQConfiguration()
        {

        }

        public static RabbitMQConfiguration Default => new RabbitMQConfiguration();
        
    }

}
