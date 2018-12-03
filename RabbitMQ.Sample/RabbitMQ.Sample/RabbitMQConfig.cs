using System;

namespace RabbitMQ.Sample
{
    public class RabbitMQConfig
    {
        public string RabbitMQHost { get; set; }
        public int? RabbitMQPort { get; set; }
        public string RabbitMQUserName { get; set; }
        public string RabbitMQPassword { get; set; }
        /// <summary>
        /// (Milliseconds) Maximun time to wait for a message to be returned from a broker when subscribe to recieve a message. 
        /// </summary>
        public int? MaxWaitTimeMilliseconds { get; set; }
        /// <summary>
        /// Defines the max number of unacknowledged deliveries that are permitted on a channel.
        /// Once the number reaches the configured count, RabbitMQ will stop delivering more messages on the channel unless at least one of the outstanding ones is acknowledged.
        /// </summary>
        public ushort? PrefetchCount { get; set; }
        public bool? LazyQueue { get; set; }

        public void Verify()
        {
            if (string.IsNullOrWhiteSpace(RabbitMQHost))
                throw new ArgumentNullException(nameof(RabbitMQHost));
            if (string.IsNullOrWhiteSpace(RabbitMQUserName))
                throw new ArgumentNullException(nameof(RabbitMQUserName));
            if (string.IsNullOrWhiteSpace(RabbitMQPassword))
                throw new ArgumentNullException(nameof(RabbitMQPassword));

            if (!RabbitMQPort.HasValue)
                throw new ArgumentNullException(nameof(RabbitMQPort));
            if (!MaxWaitTimeMilliseconds.HasValue)
                throw new ArgumentNullException(nameof(MaxWaitTimeMilliseconds));
            if (!PrefetchCount.HasValue)
                throw new ArgumentNullException(nameof(PrefetchCount));
            if (!LazyQueue.HasValue)
                throw new ArgumentNullException(nameof(LazyQueue));
        }
    }
}
