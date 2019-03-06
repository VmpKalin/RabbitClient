using RabbitMQ.Client;
using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace YxRadio.Common.Logger.RabbitMQSettings
{
    public class RabbitMQClient : IRabbitMQClient, IDisposable
    {
        private readonly RabbitMQConfiguration _config;
        private readonly PublicationAddress _publicationAddress;
        private IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _model;
        private IBasicProperties _properties;
        
        public RabbitMQClient(RabbitMQConfiguration configuration)
        {
            _config = configuration;
            _publicationAddress = new PublicationAddress(_config.ExchangeType, _config.Exchange, _config.RouteKey);
            
            InitializeEndpoint();
        }

        #region public

        public void Publish<T>(T model, string exchange, string routingKey)
        {
            using (var connection = _connectionFactory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: exchange,
                        type: "direct");

                    channel.BasicPublish(exchange: exchange,
                        routingKey: routingKey,
                        basicProperties: null,
                        body: GetReadyMessage(model));
                }
        }

        public void Publish<T>(T model) =>
            _model.BasicPublish(_publicationAddress, _properties, GetReadyMessage(model));

        public void Handle<T>(string exchange, string routingKey, Action<T> dataHandler)
        {
            _model.ExchangeDeclare(exchange: exchange, type: "topic");

            var queueName = _model.QueueDeclare().QueueName;

            _model.QueueBind(queue: ".ranger.3a1a7f980d686972",
                exchange: exchange,
                routingKey: routingKey);

            var consumer = new EventingBasicConsumer(_model);

            consumer.Received += (model, ea) => dataHandler(GetDataFromBytes<T>(ea.Body));

            _model.BasicConsume(queue: ".ranger.3a1a7f980d686972", /*consumerTag: "bunny-1551801421000-327363397220",*/
                autoAck: true,
                consumer: consumer);
        }

        public void Dispose()
        {
            _model.Dispose();
            _connection.Dispose();
        }

        #endregion

        #region private

        private IConnectionFactory GetConnectionFactory()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = _config.Hostname,
                UserName = _config.Username,
                Password = _config.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(2)
            };

            SetConfigurations(connectionFactory);

            return connectionFactory;
        }

        private void InitializeEndpoint()
        {
            _connectionFactory = GetConnectionFactory();
            _connection = _connectionFactory.CreateConnection(_config.Hostname);
            _model = _connection.CreateModel();

            _properties = _model.CreateBasicProperties();
            _properties.DeliveryMode = (byte)_config.DeliveryMode;
        }

        private T GetDataFromBytes<T>(byte[] bytes)
        {
            var message = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(message);
        }

        private byte[] GetReadyMessage<T>(T data)
        {
            var jsonSerialize = JsonConvert.SerializeObject(data);

            return Encoding.UTF8.GetBytes(jsonSerialize);
        }

        private void SetConfigurations(ConnectionFactory connectionFactory)
        {
            if (_config.Port > 0)
                connectionFactory.Port = _config.Port;

            if (!string.IsNullOrEmpty(_config.VHost))
                connectionFactory.VirtualHost = _config.VHost;

            if (_config.Protocol != null)
                connectionFactory.Protocol = _config.Protocol;
        }

        #endregion

    }

}
