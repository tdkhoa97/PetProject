using Business.Models;
using Confluent.Kafka;
using KafkaConsumer.Kafka;
using KafkaConsumer.Models;
using MassTransit;

namespace KafkaConsumer;

public class MassTransitBusHost: BusHost
{
    public MassTransitBusHost(IConfiguration configuration,
        IHostEnvironment env,
        // IHost host,
        IServiceProvider services) 
        : base(configuration, env, services)
    {
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.AddRider(rider =>
            {
                rider.AddConsumer<TestConsumer>();

                rider.UsingKafka((context, kafkaFactory) =>
                {
                    var kafkaSettings = Configuration.GetSection("Kafka").Get<KafkaSettings>();
                    if (kafkaSettings == null)
                        throw new InvalidOperationException("Kafka configuration missing.");
                    
                    var sslCaFullPath = Path.Combine(AppContext.BaseDirectory, kafkaSettings.SslCaLocation);

                    kafkaFactory.SecurityProtocol = SecurityProtocol.Ssl;
                    kafkaFactory.Host(kafkaSettings.BootstrapServers, h =>
                    {
                        // Nếu cần security credential thì add ở đây
                        h.UseSsl(cfg =>
                        {
                            cfg.CaLocation = sslCaFullPath;
                            cfg.EndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None;
                            cfg.EnableCertificateVerification = true;
                        });
                    });
                    
                    kafkaFactory.TopicEndpoint<TestMessage>(
                        topicName: "test-topic", "test-group", e =>
                        {
                            e.ConfigureConsumer<TestConsumer>(context);
                        });
                });
            });

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });
        
        // AddKafkaConfig(services);
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
    
    
    public override void ConfigureBusHandler(string topic, IServiceProvider provider, IKafkaTopicReceiveEndpointConfigurator configurator)
    {
        
    }
    
    private void AddKafkaConfig(IServiceCollection services)
    {
        services.AddKafkaBuilder(Configuration)
            .AddJsonValueProducer<Null, byte[]>()
            .AddProducer<byte[], byte[]>();

        if (Env.IsDevelopment())
        {
            ConfigureKafkaConsumers(services);
        }
    }
    
    private void ConfigureKafkaConsumers(IServiceCollection services)
    {
        Console.WriteLine("Topic TestGroup: {Topic}", Configuration["Kafka:Topics:TestGroup"]);

        
        services.AddKafkaBuilder(Configuration)
            .AddMultiJsonHandler<TestConsumer>(
                Configuration["Kafka:Topics:TestGroup"],
                config => { config.AutoOffsetReset = AutoOffsetReset.Latest; })
            ;
    }
    
    public override async Task ServiceStart(CancellationToken cancellationToken)
    {
        Console.WriteLine("🚀 Starting Kafka Bus Host...");
        
        // Check Kafka health first
        // await CheckKafkaHealthAsync(cancellationToken);
        
        // Additional startup logic
        Console.WriteLine("📋 Kafka consumers configured");
        Console.WriteLine($"🌍 Environment: {Env.EnvironmentName}");
        
        await base.ServiceStart(cancellationToken);
    }

    public override async Task ServiceStop(CancellationToken cancellationToken)
    {
        Console.WriteLine("🛑 Stopping Kafka Bus Host...");
        await base.ServiceStop(cancellationToken);
        Console.WriteLine("✅ Kafka Bus Host stopped");
    }
}