using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using CR.Infrastructure.Logging;

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

        public static byte[] CompressData(string inputXml, ILoggingService loggingService)
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

            loggingService.LogInfo(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "Compressed string input.");

            return compressedBytes;
        }

        public static byte[] CompressData(System.Xml.XmlElement inputXml, ILoggingService loggingService)
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

            loggingService.LogInfo(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "Compressed string input.");

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

        public static string ReverseString(string s)
        {
            // credit: http://www.dotnetperls.com/reverse-string

            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        public static string ReplaceFirst(string text, string search, string replace)
        {
            // credit: http://stackoverflow.com/questions/8809354/replace-first-occurrence-of-pattern-in-a-string

            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
        public static long DateTimeToUnixTimestamp(DateTime inputDate)
        {
            return (long)(inputDate - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
        }

    }
    

}
