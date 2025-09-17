namespace KafkaConsumer.Kafka.Interface;

public interface IConsumerMetadata
{
    IEnumerable<string> Topics { get; }

    int Concurrency { get; }

    int MaxErrorRetries { get; }

    int ErrorRetryBackoff { get; }

    bool ContinueAfterError { get; }

    int CommitPeriod { get; }

    int TimeoutInMiliSeconds { get; }
}