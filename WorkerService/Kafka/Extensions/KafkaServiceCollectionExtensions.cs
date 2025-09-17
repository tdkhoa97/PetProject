using Confluent.Kafka;
using KafkaConsumer;
using KafkaConsumer.Kafka;
using KafkaConsumer.Kafka.Interface;
using KafkaConsumer.Models;

namespace Microsoft.Extensions.DependencyInjection;

public static class KafkaServiceCollectionExtensions
{
    /// <summary>
    /// Register a bus host as both singleton and hosted service
    /// </summary>
    public static IServiceCollection AddBusHost<T>(this IServiceCollection services) 
        where T : BusHost
    {
        services.AddSingleton<T>();
        services.AddHostedService<T>(provider => provider.GetRequiredService<T>());
        
        return services;
    }
    
    /// <summary>
    /// Register Kafka bus host with automatic service configuration
    /// </summary>
    public static IServiceCollection AddKafkaBusHost<T>(this IServiceCollection services) 
        where T : MassTransitBusHost
    {
        // Register the bus host
        services.AddBusHost<T>();
        
        // Configure services using the bus host
        var serviceProvider = services.BuildServiceProvider();
        var busHost = serviceProvider.GetRequiredService<T>();
        busHost.ConfigureServices(services);
        
        return services;
    }
    
    public static IKafkaBuilder AddKafkaBuilder(this IServiceCollection services, IConfiguration configuration)
    {
        var kafkaSettings = configuration.GetSection("Kafka").Get<KafkaSettings>();
        if (kafkaSettings == null)
            return null;
        
        var sslCaFullPath = Path.Combine(AppContext.BaseDirectory, kafkaSettings.SslCaLocation);

        var config = new ConsumerConfig()
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            GroupId = kafkaSettings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Latest,
            SecurityProtocol = SecurityProtocol.Ssl,
            SslCaLocation = sslCaFullPath,
            EnableSslCertificateVerification = true,
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None
        };
        services.AddSingleton(config);
        
        return new KafkaBuilder(services, configuration);
    }
}

public class KafkaBuilder : IKafkaBuilder
{
    public IServiceCollection Services { get; }
    public IConfiguration Configuration { get; }

    public KafkaBuilder(IServiceCollection services, IConfiguration configuration)
    {
        Services = services;
        Configuration = configuration;
    }
}

public static class KafkaBuilderExtensions
{
    public static IKafkaBuilder AddJsonValueProducer<TKey, TValue>(this IKafkaBuilder builder)
    {
        return builder;
    }

    public static IKafkaBuilder AddProducer<TKey, TValue>(this IKafkaBuilder builder)
    {
        return builder;
    }

    public static IKafkaBuilder AddJsonValueConsumerHost<TConsumer, TKey, TValue>(
        this IKafkaBuilder builder, 
        string consumerGroup, 
        Action<ConsumerConfig> configureConsumer)
        where TConsumer : class
    {
        return builder;
    }

    public static IKafkaBuilder AddMultiJsonHandler<THandler>(
        this IKafkaBuilder builder,
        string consumerGroup, 
        Action<ConsumerConfig> configureConsumer)
        where THandler : class
    {
        return builder;
    }
}

public interface IKafkaBuilder
{
    IServiceCollection Services { get; }
    IConfiguration Configuration { get; }
}

public interface IKafkaControl
{
    Task ScheduleEveryMinute<T>(string topic, string messageType, T message, int intervalMinutes);
    Task ScheduleEveryDayAt<T>(string topic, string messageType, T message, int hour);
    Task PublishToTopic<T>(string topic, T message);
}