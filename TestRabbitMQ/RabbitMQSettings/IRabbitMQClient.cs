using System;

namespace YxRadio.Common.Logger.RabbitMQSettings
{
    public interface IRabbitMQClient
    {
        void Handle<T>(string exchange, string routingKey, Action<T> dataHandler);
        void Publish<T>(T model);
        void Publish<T>(T model, string exchange, string routingKey);
    }
}