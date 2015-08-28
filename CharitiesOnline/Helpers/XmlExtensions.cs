using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace CharitiesOnline
{
    // From http://stackoverflow.com/questions/1508572/converting-xdocument-to-xmldocument-and-vice-versa
    public static class XmlExtensions
    {
        public static XmlDocument ToXmlDocument(this XDocument xd)
        {
            var xmlDocument = new XmlDocument();
            using(var xmlReader = xd.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using(var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }

        /// <summary>
        /// Helper to get the size of an XmlDocument by getting its bytes
        /// </summary>
        /// <param name="inputDocument"></param>
        /// <returns></returns>
        public static byte[] XmlToBytes(this XmlDocument inputDocument)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(inputDocument.InnerXml);
            return bytes;
        }

    }
}
