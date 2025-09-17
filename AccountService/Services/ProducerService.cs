using Business.Models;
using Confluent.Kafka;
using KafkaConsumer.Models;

namespace AccountService.Services;

public class ProducerService
{
    private readonly ILogger<ProducerService> _logger;
    private readonly IConfiguration _configuration;
    
    public ProducerService(
        IConfiguration configuration,
        ILogger<ProducerService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ProduceAsync(CancellationToken cancellationToken)
    {
        var kafkaSettings = _configuration.GetSection("Kafka").Get<KafkaSettings>();
        if (kafkaSettings == null)
            return;
        
        var sslCaFullPath = Path.Combine(AppContext.BaseDirectory, kafkaSettings.SslCaLocation);
        var config = new ProducerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            SecurityProtocol = SecurityProtocol.Ssl,
            SslCaLocation = sslCaFullPath, 
            EnableSslCertificateVerification = true,
            Acks = Acks.All,
            AllowAutoCreateTopics = true,
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None
        };
        
        using var producer = new ProducerBuilder<byte[], byte[]>(config).Build();
        try
        {
            var keyBytes = System.Text.Encoding.UTF8.GetBytes("key-1"); // Nếu không cần key thì để null
            var testMessage = new TestMessage
            {
                Id = Guid.NewGuid().ToString(),
                Content = "Hello World!",
            };
            var jsonString = System.Text.Json.JsonSerializer.Serialize(testMessage);
            var valueBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
            // var valueBytes = System.Text.Encoding.UTF8.GetBytes($"Hello World! {DateTime.UtcNow}");
            
            var deliveryResult = await producer.ProduceAsync("test-topic", new Message<byte[], byte[]>
            {
                Key = keyBytes,   // có thể là null nếu không cần
                Value = valueBytes
            }, cancellationToken);
            
            var deliveredValue = deliveryResult.Value != null
                ? System.Text.Encoding.UTF8.GetString(deliveryResult.Value)
                : string.Empty;
            _logger.LogInformation($"Delivered '{deliveredValue}' to '{deliveryResult.Offset}'");
        }
        catch (ProduceException<Null, string> ex)
        {
            _logger.LogError($"Delivery failed: {ex.Error.Reason}");
        }
        
        producer.Flush(cancellationToken);
    }
}