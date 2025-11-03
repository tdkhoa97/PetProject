using MassTransit;

namespace KafkaConsumer.Kafka;

public abstract class BusHost: IHostedService
{
    public IConfiguration Configuration { get; }
    public IHostEnvironment Env { get; }
    protected IServiceProvider Services { get; } 


    protected BusHost(IConfiguration configuration,
        IHostEnvironment env, 
        IServiceProvider services
        )
    {
        Configuration = configuration  ?? throw new ArgumentNullException(nameof(configuration));;
        Env = env ?? throw new ArgumentNullException(nameof(env));
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <summary>
    /// Configure services for dependency injection
    /// </summary>
    public abstract void ConfigureServices(IServiceCollection services);

    // Nếu muốn handle endpoint custom (Kafka/Rabbit tuỳ bạn)
    // public abstract void ConfigureBusHandler(
    //     string topic,
    //     IServiceProvider provider,
    //     IKafkaTopicReceiveEndpointConfigurator configurator);
    
    /// <summary>
    /// Called after bus starts - override for custom startup logic
    /// </summary>
    // public virtual Task ServiceStart(CancellationToken cancellationToken) => Task.CompletedTask;
    /// <summary>
    /// Called before bus stops - override for custom cleanup logic
    /// </summary>
    // public virtual Task ServiceStop(CancellationToken cancellationToken) => Task.CompletedTask;

    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // try
        // {
        //     var service = Services.GetService<IBusControl>();
        //     // var service = Host.Services.GetService<IBusControl>();
        //     if (service != null)
        //     {
        //         await service.StartAsync(cancellationToken);
        //     }
        //
        //     await ServiceStart(cancellationToken);
        // }
        // catch (Exception ex)
        // {
        //     throw new InvalidOperationException($"Failed to start {GetType().Name}: {ex.Message}", ex);
        // }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // try
        // {
        //     await ServiceStop(cancellationToken);
        // }
        // catch (Exception ex)
        // {
        //     // Log but don't throw during shutdown
        //     Console.WriteLine($"Error during {GetType().Name} shutdown: {ex.Message}");
        // }
        // finally
        // {
        //     var service = Services.GetService<IBusControl>();
        //     // var service = Host.Services.GetService<IBusControl>();
        //     if (service != null)
        //         await service.StopAsync(cancellationToken);
        // }
    }
}