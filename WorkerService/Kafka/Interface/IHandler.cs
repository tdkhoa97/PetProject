namespace KafkaConsumer.Kafka.Interface;

public interface IHandler<T>
{
    Task Handle(HandleContext<T> context, CancellationToken cancelToken);
}