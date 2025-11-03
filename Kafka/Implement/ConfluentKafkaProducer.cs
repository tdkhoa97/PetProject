using Avro.Specific;
using Confluent.Kafka;
using Kafka.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kafka.Implement;

public class ConfluentKafkaProducer<TKey, TValue> : IKafkaProducer<TKey, TValue>
{
    private readonly IProducer<TKey, byte[]> _producer;
    private readonly ILogger _logger;

    public ConfluentKafkaProducer(ProducerConfig config, ILogger<ConfluentKafkaProducer<TKey, TValue>> logger)
    {
        _logger = logger;
        _producer = new ProducerBuilder<TKey, byte[]>(config)
            // optionally set serializers if using built-in
            .SetErrorHandler((_, e) => _logger.LogError("Kafka Producer error: {Reason}", e.Reason))
            .Build();
    }

    public async Task<DeliveryResult<TKey, byte[]>> ProduceAsync<TValue>(string topic, TValue value, CancellationToken ct = default)
        => await ProduceAsync(topic, default!, value, ct);
    
    public async Task<DeliveryResult<TKey, byte[]>> ProduceAsync<TValue>(string topic, TKey key, TValue value, CancellationToken ct = default)
    {
        byte[] payload = value switch
        {
            null => Array.Empty<byte>(),
            ISpecificRecord avroRecord => AvroSerializer.Serialize(avroRecord),
            byte[] bytes => bytes,
            _ => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value)
        };

        var message = new Message<TKey, byte[]>
        {
            Key = key,
            Value = payload
        };

        try
        {
            var result = await _producer.ProduceAsync(topic, message, ct);
            _logger.LogInformation("Produced to {Topic} partition {Partition} offset {Offset}", result.Topic, result.Partition, result.Offset);
            return result;
        }
        catch (ProduceException<TKey, byte[]> ex)
        {
            _logger.LogError(ex, "Produce failed: {Reason}", ex.Error.Reason);
            throw;
        }
    }

    public ValueTask DisposeAsync()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
        return ValueTask.CompletedTask;
    }
}