using MassTransit;

namespace KafkaConsumer.Kafka;

public abstract class BusHost: IHostedService
{
    public IConfiguration Configuration { get; }
    public IHostEnvironment Env { get; }
    public IHost Host { get; }
    
    protected BusHost(IConfiguration configuration, IHostEnvironment env, IHost host)
    {
        Configuration = configuration;
        Env = env;
        Host = host;
    }

    // Đăng ký consumer
    public abstract void ConfigureBusConsumers(IBusRegistrationConfigurator services);

    // Đăng ký service thường
    public abstract void ConfigureServices(IServiceCollection services);

    // Nếu muốn handle endpoint custom (Kafka/Rabbit tuỳ bạn)
    public abstract void ConfigureBusHandler(
        string topic,
        IServiceProvider provider,
        IKafkaTopicReceiveEndpointConfigurator configurator);
    
    public virtual Task ServiceStart(CancellationToken cancellationToken) => Task.CompletedTask;
    public virtual Task ServiceStop(CancellationToken cancellationToken) => Task.CompletedTask;

    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var service = Host.Services.GetService<IBusControl>();
        if (service != null)
        {
            await service.StartAsync(cancellationToken);
        }

        await ServiceStart(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await ServiceStop(cancellationToken);
        }
        finally
        {
            var service = Host.Services.GetService<IBusControl>();
            if (service != null)
                await service.StopAsync(cancellationToken);
        }
    }
}