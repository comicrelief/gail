using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace CharitiesOnline.Helpers
{
    public class CommonUtilityHelpers
    {
        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] CompressData(string inputXml)
        {
            var bytes = Encoding.UTF8.GetBytes(inputXml);

            byte[] compressedBytes;

            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(bytes, 0, bytes.Length);
                zipStream.Close();

                compressedBytes = compressedStream.ToArray();
            }

            return compressedBytes;
        }

        public static byte[] CompressData(System.Xml.XmlElement inputXml)
        {
            var bytes = Encoding.UTF8.GetBytes(inputXml.OuterXml);

            byte[] compressedBytes;

            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(bytes, 0, bytes.Length);
                zipStream.Close();

                compressedBytes = compressedStream.ToArray();
            }

            return compressedBytes;
        }

        public static string DecompressData(byte[] inputXml)
        {
            //var bytes = Encoding.UTF8.GetBytes(inputXml);

            using (var msi = new MemoryStream(inputXml))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CommonUtilityHelpers.CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
    }
    

}
