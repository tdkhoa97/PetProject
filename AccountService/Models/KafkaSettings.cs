namespace KafkaConsumer.Models;

public class KafkaSettings
{
    public string BootstrapServers { get; set; }
    public string GroupId { get; set; }
    public string SslCaLocation { get; set; }
    public bool EnableSslCertificateVerification { get; set; }
}