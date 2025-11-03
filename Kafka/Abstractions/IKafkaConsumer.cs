using Confluent.Kafka;

namespace Kafka.Abstractions;

public interface IKafkaConsumer<TKey, TValue> : IAsyncDisposable
{
    /// <summary>
    /// Start polling loop; handler returns Task for message processing.
    /// </summary>
    Task StartAsync(Func<ConsumeResult<TKey, byte[]>, Task> messageHandler, CancellationToken ct);
    
    Task PauseAsync();
    Task ResumeAsync();
}