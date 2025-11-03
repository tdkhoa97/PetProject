using Business.Models;
using Kafka.Abstractions;

namespace AccountService.Services;

public class ProducerService
{
    private readonly ILogger<ProducerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IKafkaProducer<byte[], byte[]> _producer;

    public ProducerService(
        IKafkaProducer<byte[], byte[]> producer)
    {
        _producer = producer;
    }

    public async Task ProduceAsync(string content, CancellationToken cancellationToken)
    {
        var msg = new TestMessage
        {
            Id = Guid.NewGuid().ToString(), Content = string.IsNullOrEmpty(content) 
                ? "Hello"
                : content
        };
        await _producer.ProduceAsync("test-topic", msg, cancellationToken); // null key ok if TKey is byte[]
    }
}