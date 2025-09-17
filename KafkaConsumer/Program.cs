using KafkaConsumer;
using MassTransit;

// var builder = WebApplication.CreateBuilder(args);
var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
// Tạo MassTransitBusHost
    var busHost = new MassTransitBusHost(
        context.Configuration,
        context.HostingEnvironment,
        services.BuildServiceProvider() // tạm serviceProvider
    );
    
    busHost.ConfigureServices(services);
    services.AddSingleton(busHost);
    services.AddHostedService(provider => provider.GetRequiredService<MassTransitBusHost>());

    // services.AddSingleton<MassTransitBusHost>(provider =>
    // {
    //     var env = provider.GetRequiredService<IHostEnvironment>();
    //     var config = provider.GetRequiredService<IConfiguration>();
    //     var busHost = new MassTransitBusHost(config, env, provider);
    //     busHost.ConfigureServices(services); // hoặc chuyển configure ra ngoài
    //     return busHost;
    // });
    //
    // services.AddHostedService(provider => provider.GetRequiredService<MassTransitBusHost>());
});

var app = builder.Build();
await app.RunAsync();