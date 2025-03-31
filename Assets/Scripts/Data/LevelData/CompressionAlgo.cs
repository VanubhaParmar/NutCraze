using System.IO;
using System.IO.Compression;

namespace Tag.NutSort
{
    public class CompressionAlgo
    {
        public static byte[] Compress(byte[] data)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(outputStream, CompressionLevel.Fastest))
                {
                    gzipStream.Write(data, 0, data.Length);
                }
                return outputStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            using (MemoryStream inputStream = new MemoryStream(data))
            using (GZipStream gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (MemoryStream outputStream = new MemoryStream())
            {
                gzipStream.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }
    }
}
