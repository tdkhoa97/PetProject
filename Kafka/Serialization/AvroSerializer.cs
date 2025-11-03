using Avro.IO;
using Avro.Specific;

namespace Kafka;

public static class AvroSerializer
{
    public static byte[] Serialize<T>(T record) where T : ISpecificRecord
    {
        using var ms = new MemoryStream();
        var encoder = new BinaryEncoder(ms);
        var writer = new SpecificDatumWriter<T>(record.Schema);
        writer.Write(record, encoder);
        return ms.ToArray();
    }

    public static T Deserialize<T>(byte[] data) where T : ISpecificRecord, new()
    {
        using var ms = new MemoryStream(data);
        var decoder = new BinaryDecoder(ms);
        var instance = new T();
        var reader = new SpecificDatumReader<T>(instance.Schema, instance.Schema);
        return reader.Read(instance, decoder);
    }
}