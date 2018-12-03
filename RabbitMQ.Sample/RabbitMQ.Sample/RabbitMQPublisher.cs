using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ.Sample
{
    public class RabbitMQPublisher : IDisposable
    {
        IConnection _connection;
        IModel _channel;
        RabbitMQConfig _config;

        public RabbitMQPublisher(IOptions<RabbitMQConfig> config)
        {
            if (config == null || config.Value == null)
                throw new ArgumentNullException(nameof(config));

            config.Value.Verify();
            _config = config.Value;

            var factory = new ConnectionFactory()
            {
                HostName = _config.RabbitMQHost,
                UserName = _config.RabbitMQUserName,
                Password = _config.RabbitMQPassword,
                Port = _config.RabbitMQPort.Value
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ConfirmSelect();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="mode">lazy / default</param>
        public void Send(string queueName, string message, string exchangeName, string routingKey, bool lazy = false)
        {
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);

            var queueParams = new Dictionary<string, object>();

            if (lazy)
                queueParams.Add("mode", "lazy");

            _channel.QueueDeclare(queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: queueParams);

            _channel.QueueBind(queueName, exchangeName, routingKey, null);

            var body = Encoding.UTF8.GetBytes(message); 

            _channel.BasicPublish(exchange: exchangeName,
                routingKey: routingKey, 
                basicProperties: null,
                body: body);

        }

        public void Send(string queueName, byte[] message, string exchangeName, string routingKey, bool lazy = false)
        {
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);

            var queueParams = new Dictionary<string, object>();

            if (lazy)
                queueParams.Add("mode", "lazy");

            _channel.QueueDeclare(queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: queueParams);

            _channel.QueueBind(queueName, exchangeName, routingKey, null);

            _channel.BasicPublish(exchange: exchangeName,
                routingKey: routingKey,
                basicProperties: null,
                body: message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="mode">lazy / default</param>
        public void SendBactch(string queueName, List<string> messages, string exchangeName, string routingKey,
            bool lazy = false)
        {
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);

            var queueParams = new Dictionary<string, object>();

            if (lazy)
                queueParams.Add("mode", "lazy");

            _channel.QueueDeclare(queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: queueParams);

            _channel.QueueBind(queueName, exchangeName, routingKey, null);

            var batch = _channel.CreateBasicPublishBatch();

            foreach (var message in messages)
            {
                var body = Encoding.UTF8.GetBytes(message);
                batch.Add(exchangeName, routingKey, false, null, body);
            }

            batch.Publish();
            _channel.WaitForConfirms();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="mode">lazy / default</param>
        public void SendByteBactch(string queueName, List<byte[]> messages, string exchangeName, string routingKey,
            bool lazy = false)
        {
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);

            var queueParams = new Dictionary<string, object>();

            if (lazy)
                queueParams.Add("mode", "lazy");

            _channel.QueueDeclare(queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: queueParams);

            _channel.QueueBind(queueName, exchangeName, routingKey, null);

            var batch = _channel.CreateBasicPublishBatch();

            foreach (var message in messages)
            {
                batch.Add(exchangeName, routingKey, false, null, message);
            }

            batch.Publish();
            _channel.WaitForConfirms();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_channel != null)
                        _channel.Dispose();
                    if (_connection != null)
                        _connection.Dispose();
                }

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RabbitMQConsumer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
