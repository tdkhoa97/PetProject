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

                
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
                
                rider.UsingKafka((context, kafkaFactory) =>
                {
                    var settings = GetKafkaSettings();
                    ConfigureKafka(kafkaFactory, settings);
                    
                    kafkaFactory.TopicEndpoint<TestMessage>(
                        topicName: "test-topic", "test-group", e =>
                        {
                            e.UseExecute(ctx =>
                            {
                                if (ctx.TryGetPayload<KafkaConsumeContext>(out var cr))
                                {
                                    Console.WriteLine($"[Execute] Topic: {cr.Topic}, Partition: {cr.Partition}, Offset: {cr.Offset}");
                                }
                                
                            });
                            
                            e.UseSampleRetryConfiguration();
                            e.ConfigureConsumer<TestConsumer>(context);
                        });
                });
            });
        });
        
        // AddKafkaConfig(services);
    }
    
    private KafkaSettings GetKafkaSettings()
    {
        var settings = Configuration.GetSection("Kafka").Get<KafkaSettings>();
        if (settings == null)
            throw new InvalidOperationException("Kafka configuration missing.");
        return settings;
    }
    
    private void ConfigureKafka(IKafkaFactoryConfigurator kafka, KafkaSettings settings)
    {
        var sslCaFullPath = Path.Combine(AppContext.BaseDirectory, settings.SslCaLocation);

        kafka.SecurityProtocol = SecurityProtocol.Ssl;
        kafka.Host(settings.BootstrapServers, h =>
        {
            h.UseSsl(cfg =>
            {
                cfg.CaLocation = sslCaFullPath;
                cfg.EndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None;
                cfg.EnableCertificateVerification = true;
            });
        });
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
            // ConfigureKafkaConsumers(services);
        }
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