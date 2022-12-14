using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Core.Utils;

public static class Serializer
{
    public static byte[] ToBytes(object obj)
    {
        var formatter = new BinaryFormatter();
        using var memoryStream = new MemoryStream();
        
        formatter.Serialize(memoryStream, obj);

        return memoryStream.GetBuffer();
    }

    public static T FromBytes<T>(byte[] bytes)
    {
        var formatter = new BinaryFormatter();
        using var memoryStream = new MemoryStream(bytes);

        return (T)formatter.Deserialize(memoryStream);
    }
}