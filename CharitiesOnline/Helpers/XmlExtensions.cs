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
            xmlDocument.PreserveWhitespace = true;
            using(var xmlReader = xd.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = false;

            XmlNodeReader nodeReader = new XmlNodeReader(xmlDocument);

            using(var reader = XmlReader.Create(nodeReader,settings))
            {                
                reader.MoveToContent();

                // LoadOptions.PreserveWhitespace doesn't work with XmlNodeReader
                XDocument xdocument = XDocument.Load(reader);

                return xdocument;
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
