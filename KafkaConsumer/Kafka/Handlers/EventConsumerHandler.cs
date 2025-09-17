using Confluent.Kafka;
using KafkaConsumer.Models;

namespace KafkaConsumer;

internal class EventConsumerHandler : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EventConsumerHandler> _logger;
    private readonly IConsumer<byte[], byte[]> _consumer;

    public EventConsumerHandler(
        IConfiguration configuration,
        ILogger<EventConsumerHandler> logger
    )
    {
        _configuration = configuration;
        _logger = logger;
        var kafkaSettings = configuration.GetSection("Kafka").Get<KafkaSettings>();
        if (kafkaSettings == null)
            return;
        
        var sslCaFullPath = Path.Combine(AppContext.BaseDirectory, kafkaSettings.SslCaLocation);

        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            GroupId = kafkaSettings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Latest,
            SecurityProtocol = SecurityProtocol.Ssl,
            SslCaLocation = sslCaFullPath,
            EnableSslCertificateVerification = true,
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None
        };

        _consumer = new ConsumerBuilder<byte[], byte[]>(config)
            .SetPartitionsAssignedHandler((c, partitions) =>
            {
                _logger.LogInformation($"Assigned partitions: [{string.Join(", ", partitions)}]");
            })
            .SetPartitionsRevokedHandler((c, partitions) =>
            {
                _logger.LogInformation($"Revoked partitions: [{string.Join(", ", partitions)}]");
            })
            .Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
       
        _consumer.Subscribe("test-topic");
        // _consumer.OnPartitionsAssigned += (_, partitions) =>
        // {
        //     _logger.LogInformation($"Assigned partitions: [{string.Join(", ", partitions)}]");
        // };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(10));
                if (consumeResult == null)
                {
                    await Task.Delay(1000, stoppingToken); // chờ 1 giây trước khi retry
                    continue;
                }
                
                // Convert byte[] -> string nếu cần
                // Convert byte[] -> string nếu cần
                var keyString = consumeResult.Message.Key != null
                    ? System.Text.Encoding.UTF8.GetString(consumeResult.Message.Key)
                    : null;
                var valueString = consumeResult.Message.Value != null
                    ? System.Text.Encoding.UTF8.GetString(consumeResult.Message.Value)
                    : null;

                _logger.LogInformation($"Consumed message at {consumeResult.TopicPartitionOffset}. Key: {keyString}, Value: {valueString}");
            }
            catch(OperationCanceledException ex)
            {
                _logger.LogError(ex.Message);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError($"Consume error: {ex.Error.Reason}");
                await Task.Delay(2000, stoppingToken); // chờ trước khi retry
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                await Task.Delay(5000, stoppingToken); // backoff lâu hơn nếu lỗi lạ
            }
        }
    }
}