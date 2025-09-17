using Business.Models;
using KafkaConsumer.Kafka;
using KafkaConsumer.Kafka.Interface;
using MassTransit;

namespace KafkaConsumer;

// [ConsumerMetadata(new[] { "test-topic" }, 1)]
public class TestConsumer : IConsumer<TestMessage>
{
    private readonly IServiceProvider _provider;

    public TestConsumer(IServiceProvider provider)
    {
        _provider = provider;
    }
    
    public async Task Consume(ConsumeContext<TestMessage> context)
    {
        var message = context.Message; // TestMessage object
        
        Console.WriteLine($"Processing TestMessage - Id: {message.Id}, Content: {message.Content}"); 
        
        await Task.CompletedTask;
    }
}