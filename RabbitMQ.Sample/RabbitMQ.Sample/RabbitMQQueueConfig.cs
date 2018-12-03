using System;

namespace RabbitMQ.Sample
{
    public class RabbitMQQueueConfig
    {
        public string QueueName { get; set; }
        public string ExchangeKey { get; set; }
        public string RoutingKey { get; set; }
        public int? BatchSize { get; set; }

        public void Verify()
        {
            if (string.IsNullOrWhiteSpace(QueueName))
                throw new ArgumentNullException(nameof(QueueName));
            if (string.IsNullOrWhiteSpace(ExchangeKey))
                throw new ArgumentNullException(nameof(ExchangeKey));
            if (string.IsNullOrWhiteSpace(RoutingKey))
                throw new ArgumentNullException(nameof(RoutingKey));

            if (!BatchSize.HasValue)
                throw new ArgumentNullException(nameof(BatchSize));
        }
    }
}
