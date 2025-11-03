using Confluent.Kafka;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Extensions;

public static class KafkaMassTransitExtensions
{
    /// <summary>
    /// Cấu hình host Kafka dùng chung cho tất cả consumer.
    /// </summary>
    public static void ConfigureKafkaHost(
        this IKafkaFactoryConfigurator kafkaFactory,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection("Kafka").Get<Models.KafkaOptions>();
        var sslCaFullPath = Path.Combine(AppContext.BaseDirectory, settings.SslCaLocation);

        kafkaFactory.SecurityProtocol = SecurityProtocol.Ssl;
        kafkaFactory.Host(settings.BootstrapServers, h =>
        {
            h.UseSsl(cfg =>
            {
                cfg.CaLocation = sslCaFullPath;
                cfg.EndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None;
                cfg.EnableCertificateVerification = true;
            });
        });
    }
    
    /// <summary>
    /// Đăng ký 1 consumer cho topic cụ thể, dùng cấu hình Kafka host đã setup.
    /// </summary>
    public static void RegisterKafkaConsumer<TConsumer, TValue>(
        this IKafkaFactoryConfigurator kafkaFactory,
        IRiderRegistrationContext context,
        string topicName,
        string groupId,
        Action<IKafkaTopicReceiveEndpointConfigurator>? configure = null)
        where TConsumer : class, IConsumer<TValue>
        where TValue : class
    {
        kafkaFactory.TopicEndpoint<Null, TValue>(topicName, groupId, e =>
        {
            e.ConfigureConsumer<TConsumer>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(3)));
            e.UseInMemoryOutbox();
            e.CheckpointInterval = TimeSpan.FromSeconds(30);

            configure?.Invoke(e);
        });
    }
    
    /// <summary>
    /// Đăng ký Kafka consumer cho MassTransit Rider, có thể chỉ định groupId riêng.
    /// </summary>
    public static IRiderRegistrationConfigurator AddKafkaConsumerHost<TConsumer, TKey, TValue>(
        this IRiderRegistrationConfigurator rider,
        string topicName,
        string groupId,
        IConfiguration configuration,
        Action<ConsumerConfig>? configure = null)
        where TConsumer : class, IConsumer<TValue>
        where TValue : class
    {
        rider.AddConsumer<TConsumer>();

        rider.UsingKafka((context, k) =>
        {
            var settings = configuration.GetSection("Kafka").Get<Models.KafkaOptions>();
            var sslCaFullPath = Path.Combine(AppContext.BaseDirectory, settings.SslCaLocation);

            k.Host(settings.BootstrapServers, h =>
            {
                h.UseSsl(cfg =>
                {
                    cfg.CaLocation = sslCaFullPath;
                    cfg.EndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None;
                    cfg.EnableCertificateVerification = true;
                });
            });

            var consumerConfig = new ConsumerConfig
            {
                GroupId = groupId,
                BootstrapServers = settings.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Latest,
                EnableAutoCommit = true,
                SecurityProtocol = SecurityProtocol.Ssl
            };

            configure?.Invoke(consumerConfig);

            // 🔥 TopicEndpoint đăng ký group theo tham số
            k.TopicEndpoint<TKey, TValue>(topicName, groupId, e =>
            {
                e.ConfigureConsumer<TConsumer>(context);
                e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(3)));
                e.UseInMemoryOutbox();
            });
        });

        return rider;
    }
}
