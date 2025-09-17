using Business.Models;
using KafkaConsumer.Kafka;
using KafkaConsumer.Kafka.Interface;

namespace KafkaConsumer;

[ConsumerMetadata(new[] { "test-topic" }, 1)]
public class TestHandler : IHandler<TestMessage>
{
    private readonly IServiceProvider _provider;

    public TestHandler(IServiceProvider provider)
    {
        _provider = provider;
    }
    
    public async Task Handle(HandleContext<TestMessage> context, CancellationToken cancelToken)
    {
        var message = context.Message; // TestMessage object
        var key = context.Key;       // "key-1"
        
        Console.WriteLine("Processing TestMessage - Id: {Id}, Content: {Content}", 
            message.Id, message.Content);
        
        await Task.CompletedTask;
    }
}