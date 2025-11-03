using Confluent.Kafka;

namespace Kafka.Abstractions;

public interface IKafkaProducer<TKey, TValue> : IAsyncDisposable
{
    Task<DeliveryResult<TKey, byte[]>> ProduceAsync<TValue>(string topic, TValue value, CancellationToken ct = default);
    Task<DeliveryResult<TKey, byte[]>> ProduceAsync<TValue>(string topic, TKey key, TValue value, CancellationToken ct = default);
}