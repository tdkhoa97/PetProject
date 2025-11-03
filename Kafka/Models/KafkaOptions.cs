namespace Kafka.Models;

public class KafkaOptions
{
    public string BootstrapServers { get; set; } = "";
    public string Topic { get; set; } = "";
    public string SslCaLocation { get; set; } = "";
    public string ClientId { get; set; } = "";
    public bool EnableSsl { get; set; } = false;
    public int MessageTimeoutMs { get; set; } = 30000;
    public int Retries { get; set; } = 3;
}