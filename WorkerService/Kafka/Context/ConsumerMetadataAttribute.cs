using KafkaConsumer.Kafka.Interface;

namespace KafkaConsumer.Kafka;

public class ConsumerMetadataAttribute : Attribute, IConsumerMetadata
{
    public ConsumerMetadataAttribute(
        string[] Topics,
        int Concurrency = 0,
        int MaxErrorRetries = 10,
        int ErrorRetryBackoff = 3000,
        bool ContinueAfterError = false,
        int CommitPeriod = 1,
        int TimeoutInMiliSeconds = 0)
    {
        this.Topics = (IEnumerable<string>)Topics;
        this.Concurrency = Concurrency;
        this.MaxErrorRetries = MaxErrorRetries;
        this.ErrorRetryBackoff = ErrorRetryBackoff;
        this.ContinueAfterError = ContinueAfterError;
        this.CommitPeriod = CommitPeriod;
        this.TimeoutInMiliSeconds = TimeoutInMiliSeconds;
    }

    public ConsumerMetadataAttribute(
        string Topic,
        int Concurrency = 0,
        int MaxErrorRetries = 10,
        int ErrorRetryBackoff = 3000,
        bool ContinueAfterError = false,
        int CommitPeriod = 1,
        int TimeoutInMiliSeconds = 0)
        : this(new string[1] { Topic }, Concurrency, MaxErrorRetries, ErrorRetryBackoff, ContinueAfterError,
            CommitPeriod, TimeoutInMiliSeconds)
    {
    }

    public IEnumerable<string> Topics { get; }

    public int Concurrency { get; }

    public int MaxErrorRetries { get; }

    public int ErrorRetryBackoff { get; }

    public bool ContinueAfterError { get; }

    public int CommitPeriod { get; }

    public int TimeoutInMiliSeconds { get; }
}