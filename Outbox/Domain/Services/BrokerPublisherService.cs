using Confluent.Kafka;
using System.Text.Json;

namespace Outbox.Services
{
    public interface IBrokerPublisherService
    {
        Task PublishEventAsync(object payload);
    }

    public class KafkaOrderMessageProducer : IBrokerPublisherService
    {
        public KafkaOrderMessageProducer()
        {

        }

        public async Task PublishEventAsync(object payload)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:29092",
                ClientId = "my-producer",
                MessageTimeoutMs = 5000
            };

            var builder = new ProducerBuilder<Null, string>(config);
            using var producer = builder.Build();

            var message = new Message<Null, string>
            {
                Value = JsonSerializer.Serialize(payload)
            };

            await producer.ProduceAsync("outbox-playground-orders", message);
        }
    }
}
