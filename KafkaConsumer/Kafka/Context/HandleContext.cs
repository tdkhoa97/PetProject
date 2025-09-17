using Confluent.Kafka;

namespace KafkaConsumer.Kafka;

public class HandleContext<T>
{
    public HandleContext(
        byte[] key,
        T message,
        Headers headers,
        DateTime createdTime,
        string topic,
        string consumerGroup)
    {
        this.Key = key;
        this.Message = message;
        this.Headers = headers;
        this.CreatedTime = createdTime;
        this.Topic = topic;
        this.ConsumerGroup = consumerGroup;
    }

    public byte[] Key { get; }

    public T Message { get; }

    public string Topic { get; }

    public string ConsumerGroup { get; }

    public DateTime CreatedTime { get; }

    public Headers Headers { get; }

    public static HandleContext<T> Create<T>(
        byte[] key,
        T message,
        Headers headers,
        DateTime createdTime,
        string topic,
        string consumerGroup)
    {
        return new HandleContext<T>(key, message, headers, createdTime, topic, consumerGroup);
    }
}