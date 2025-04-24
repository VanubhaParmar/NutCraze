using ProtoBuf;
using System.IO;

namespace Tag.NutSort
{
    public class ProtoBufHelper
    {
        public static byte[] Serialize<T>(T data)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, data);
                return stream.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize<T>(stream);
            }
        }

        // If you want to serialize to a file
        public static void SerializeToFile<T>(T data, string filePath)
        {
            using (var file = File.Create(filePath))
            {
                Serializer.Serialize(file, data);
            }
        }

        // If you want to deserialize from a file
        public static T DeserializeFromFile<T>(string filePath)
        {
            using (var file = File.OpenRead(filePath))
            {
                return Serializer.Deserialize<T>(file);
            }
        }
    }
}
