using Confluent.Kafka;
using KafkaConsumer;
using KafkaConsumer.Kafka;
using KafkaConsumer.Models;
using MassTransit;
using Serilog;
using Serilog.Events;

namespace Microsoft.Extensions.DependencyInjection;

public static class KafkaServiceExtensions
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
    
    /// <summary>
    /// Just an example, but some retry/kill switch combination to stop processing when the consumer/saga faults repeatedly.
    /// </summary>
    /// <param name="configurator"></param>
    public static void UseSampleRetryConfiguration(this IKafkaTopicReceiveEndpointConfigurator configurator)
    {
        configurator.UseKillSwitch(k => k.SetActivationThreshold(1).SetRestartTimeout(m: 1).SetTripThreshold(0.2).SetTrackingPeriod(m: 1));
        configurator.UseMessageRetry(retry => retry.Interval(1000, TimeSpan.FromSeconds(1)));
    }
    
    public static T ConfigureSerilog<T>(this T builder)
        where T : IHostBuilder
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        builder.UseSerilog();

        return builder;
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