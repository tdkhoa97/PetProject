using Confluent.Kafka;

namespace KafkaConsumer.Kafka.Interface;

public interface IConsumerMessage<TKey, TValue>
{
    Task Consume(ConsumeResult<TKey, TValue> result, CancellationToken token);
}