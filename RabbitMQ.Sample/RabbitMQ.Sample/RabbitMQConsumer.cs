using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RabbitMQ.Sample
{
    public class RabbitMQConsumer : IDisposable
    {
        IConnection _connection;
        IModel _channel;
        RabbitMQConfig _config;

        private readonly TimeSpan _maxWait;

        public delegate void ProcessingDelegate(string message);

        public RabbitMQConsumer(IOptions<RabbitMQConfig> config)
        {
            if (config == null || config.Value == null)
               throw new ArgumentNullException(nameof(config));

            config.Value.Verify();
            _config = config.Value;

            var factory = new ConnectionFactory()
            {
                HostName = _config.RabbitMQHost, UserName = _config.RabbitMQUserName, Password = _config.RabbitMQPassword,
                Port = _config.RabbitMQPort.Value
            };
            _maxWait = TimeSpan.FromMilliseconds(_config.MaxWaitTimeMilliseconds.Value);
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.BasicQos(prefetchSize:0, prefetchCount: _config.PrefetchCount.Value, global: true);
        }

        public KeyValuePair<ulong, string> GetMessage(string queueName)
        {
            bool noAck = false;
            BasicGetResult queueItem = _channel.BasicGet(queueName, noAck);
            
            if (queueItem == null)
            {
                return new KeyValuePair<ulong, string>();
            }
            else
            {
                IBasicProperties props = queueItem.BasicProperties;
                byte[] body = queueItem.Body;

                var message = Encoding.UTF8.GetString(body);
                var result = new KeyValuePair<ulong, string>(queueItem.DeliveryTag, message);
                
                return result;
            }
        }

        public KeyValuePair<ulong, byte[]> GetByteMessage(string queueName)
        {
            bool noAck = false;
            BasicGetResult queueItem = _channel.BasicGet(queueName, noAck);

            if (queueItem == null)
            {
                return new KeyValuePair<ulong, byte[]>();
            }
            else
            {
                IBasicProperties props = queueItem.BasicProperties;
                byte[] body = queueItem.Body;

                var result = new KeyValuePair<ulong, byte[]>(queueItem.DeliveryTag, body);

                return result;
            }
        }

        public void ConfirmProcessedMessage(ulong deliveryTag)
        {
            _channel.BasicAck(deliveryTag: deliveryTag, multiple: false);
        }

        public void Requeue(ulong deliveryTag)
        {
            _channel.BasicNack(deliveryTag: deliveryTag, multiple: false, requeue: true);
        }

        public void RemoveFromQueue(ulong deliveryTag)
        {
            _channel.BasicReject(deliveryTag, false);
        }

        public void SubscribeToMessages(string queueName, string exchangeName, string routingKey, ProcessingDelegate processingMethod)
        {
            _channel.ExchangeDeclare(exchange: exchangeName,
                type: "direct");

            _channel.QueueDeclare(queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueBind(queue: queueName,
                exchange: exchangeName,
                routingKey: routingKey);

            var consumer = new EventingBasicConsumer(_channel);
            var _messageReceived = new AutoResetEvent(false);

            consumer.Received += (model, e) =>
            {
                try
                {
                    var body = e.Body;
                    var message = Encoding.UTF8.GetString(body);

                    processingMethod(message);

                    _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                }
                catch (Exception exception)
                {
                    //TODO Log error

                    //return message to queue in case if it was not processed
                    _channel.BasicReject(e.DeliveryTag, true);
                }

                _messageReceived.Set();
            };

            _channel.BasicConsume(queue: queueName,
                autoAck: false,
                consumer: consumer);

            _messageReceived.WaitOne(_maxWait);
        }

        public uint QueueLength(string name)
        {
            return _channel.MessageCount(name);
        }

        public void RemoveQueue(string name)
        {
            _channel.QueueDelete(name);
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
                    {
                        _channel.Dispose();
                    }

                    if (_connection != null)
                    {
                        _connection.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
