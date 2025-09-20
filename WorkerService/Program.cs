using KafkaConsumer;

// var builder = WebApplication.CreateBuilder(args);
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureSerilog();

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

// Bật log console
// builder.ConfigureLogging(logging =>
// {
//     logging.ClearProviders();
//     logging.AddConsole(); // hiển thị log ra console
//     logging.SetMinimumLevel(LogLevel.Debug); // bật mức Debug
// });

var app = builder.Build();
await app.RunAsync();