using Confluent.Kafka;
using Kafka.Abstractions;
using Kafka.Implement;
using Kafka.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfluentKafka(this IServiceCollection services, IConfiguration config)
    {
        var kafkaOptions = new KafkaOptions();
        config.GetSection("Kafka").Bind(kafkaOptions);
        var sslCaFullPath = Path.Combine(AppContext.BaseDirectory, kafkaOptions.SslCaLocation);

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = kafkaOptions.BootstrapServers,
            SecurityProtocol = SecurityProtocol.Ssl,
            SslCaLocation = sslCaFullPath, 
            EnableSslCertificateVerification = true,
            Acks = Acks.All,
            AllowAutoCreateTopics = true,
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None

        };

        var consumerConfig = new ConsumerConfig()
        {
            BootstrapServers = kafkaOptions.BootstrapServers,
            GroupId = config.GetValue<string>("Kafka:GroupId") ?? "default-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        services.AddSingleton(producerConfig);
        services.AddSingleton(consumerConfig);

        services.AddSingleton(typeof(IKafkaProducer<,>), typeof(ConfluentKafkaProducer<,>));
        services.AddSingleton(typeof(IKafkaConsumer<,>), typeof(ConfluentKafkaConsumer<,>));

        return services;
    }
}