using Business.Models;
using Confluent.Kafka;
using Kafka.Extensions;
using KafkaConsumer.Kafka;
using KafkaConsumer.Models;
using MassTransit;

namespace KafkaConsumer;

public class MassTransitBusHost : BusHost
{
    private readonly IConfiguration _configuration;

    public MassTransitBusHost(IConfiguration configuration,
        IHostEnvironment env,
        // IHost host,
        IServiceProvider services) 
        : base(configuration, env, services)
    {
        _configuration = configuration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            // Đăng ký tất cả consumer trước
            x.AddConsumer<TestConsumer>();

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
            
            x.AddRider(rider =>
            {
                rider.AddConsumersFromNamespaceContaining<TestConsumer>();

                rider.UsingKafka((context, k) =>
                {
                    // Host config (chung cho tất cả)
                    k.ConfigureKafkaHost(_configuration);

                    // Đăng ký từng consumer vào topic tương ứng
                    k.RegisterKafkaConsumer<TestConsumer, TestMessage>(
                        context, "test-topic", "test-group",
                        e =>
                        {
                            e.AutoOffsetReset = AutoOffsetReset.Latest;
                        });
                });
            });
        });
    }
    
    // public override void ConfigureBusHandler(string topic, IServiceProvider provider, IKafkaTopicReceiveEndpointConfigurator configurator)
    // {
    //     
    // }

    // public override async Task ServiceStart(CancellationToken cancellationToken)
    // {
    //     Console.WriteLine("🚀 Starting Kafka Bus Host...");
    //     
    //     // Check Kafka health first
    //     // await CheckKafkaHealthAsync(cancellationToken);
    //     
    //     // Additional startup logic
    //     Console.WriteLine("📋 Kafka consumers configured");
    //     Console.WriteLine($"🌍 Environment: {Env.EnvironmentName}");
    //     
    //     await base.ServiceStart(cancellationToken);
    // }
    //
    // public override async Task ServiceStop(CancellationToken cancellationToken)
    // {
    //     Console.WriteLine("🛑 Stopping Kafka Bus Host...");
    //     await base.ServiceStop(cancellationToken);
    //     Console.WriteLine("✅ Kafka Bus Host stopped");
    // }
}