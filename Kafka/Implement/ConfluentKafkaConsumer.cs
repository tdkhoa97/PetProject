using Kafka.Abstractions;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

namespace Kafka.Implement;

public class ConfluentKafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue>
{
    private readonly IConsumer<TKey, byte[]> _consumer;
    private readonly ILogger _logger;

    public ConfluentKafkaConsumer(ConsumerConfig config, ILogger<ConfluentKafkaConsumer<TKey, TValue>> logger)
    {
        _logger = logger;
        _consumer = new ConsumerBuilder<TKey, byte[]>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka Consumer error: {Reason}", e.Reason))
            .Build();
    }

    public async Task StartAsync(Func<ConsumeResult<TKey, byte[]>, Task> messageHandler, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(ct);
                    if (result == null) continue;

                    // run handler (not awaiting if you want parallel processing)
                    await messageHandler(result);

                    // commit if using manual commit
                    _consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Consume exception: {Reason}", ex.Error.Reason);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumer loop canceled");
        }
    }

    public Task PauseAsync()
    {
        _consumer.Pause(_consumer.Assignment);
        return Task.CompletedTask;
    }

    public Task ResumeAsync()
    {
        _consumer.Resume(_consumer.Assignment);
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _consumer.Close();
        _consumer.Dispose();
        return ValueTask.CompletedTask;
    }
}
