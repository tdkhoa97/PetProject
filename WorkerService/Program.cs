using KafkaConsumer;

// var builder = WebApplication.CreateBuilder(args);
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureSerilog();

builder.ConfigureServices((context, services) =>
{
    services.AddKafkaBusHost<MassTransitBusHost>();
});

var app = builder.Build();
await app.RunAsync();