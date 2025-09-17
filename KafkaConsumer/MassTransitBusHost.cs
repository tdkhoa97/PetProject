using KafkaConsumer.Kafka;
using MassTransit;

namespace KafkaConsumer;

public class KafkaBusHost: BusHost
{
    public KafkaBusHost(IConfiguration configuration, IHostEnvironment env, IHost host) 
        : base(configuration, env, host)
    {
    }
    
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            // Register consumers
            ConfigureConsumers(x);
            
            // Configure Kafka rider
            x.AddRider(rider =>
            {
                ConfigureKafkaRider(rider);
            });
        });
    }
    
    /// <summary>
    /// Override this method to register your consumers
    /// </summary>
    protected virtual void ConfigureConsumers(IBusRegistrationConfigurator configurator)
    {
        // Example:
        // configurator.AddConsumer<YourMessageConsumer>();
    }
    
    /// <summary>
    /// Override this method to configure your Kafka topics and endpoints
    /// </summary>
    protected virtual void ConfigureKafkaTopics(IRiderRegistrationContext context, IKafkaFactoryConfigurator kafkaConfig)
    {
        // Example:
        // kafkaConfig.TopicEndpoint<YourMessage>("your-topic", "consumer-group", e =>
        // {
        //     e.ConfigureConsumer<YourMessageConsumer>(context);
        // });
    }
    
    /// <summary>
    /// Configure Kafka rider settings
    /// </summary>
    protected virtual void ConfigureKafkaRider(IRiderRegistrationConfigurator rider)
    {
        rider.UsingKafka((context, kafkaConfig) =>
        {
            // Get Kafka settings from configuration
            var kafkaSettings = Configuration.GetSection("Kafka");
            var bootstrapServers = kafkaSettings["BootstrapServers"] ?? "localhost:9092";
            
            kafkaConfig.Host(bootstrapServers);
            
            // Configure topics and endpoints
            ConfigureKafkaTopics(context, kafkaConfig);
        });
    }
    

    public override void ConfigureBusHandler(string topic, IServiceProvider provider, IKafkaTopicReceiveEndpointConfigurator configurator)
    {
        
    }
}