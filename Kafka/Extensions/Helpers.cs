namespace Kafka.Helpers;

public static class JsonSerializerHelper
{
    public static byte[] Serialize<T>(T obj)
        => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj);

    public static T? Deserialize<T>(byte[] data)
        => System.Text.Json.JsonSerializer.Deserialize<T>(data);
}