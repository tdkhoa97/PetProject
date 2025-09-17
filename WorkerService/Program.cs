using KafkaConsumer;

// var builder = WebApplication.CreateBuilder(args);
var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    var busHost = new MassTransitBusHost(
        context.Configuration,
        context.HostingEnvironment,
        services.BuildServiceProvider() // tạm serviceProvider
    );
    
    busHost.ConfigureServices(services);
    services.AddSingleton(busHost);
    services.AddHostedService(provider => provider.GetRequiredService<MassTransitBusHost>());
});

var app = builder.Build();
await app.RunAsync();