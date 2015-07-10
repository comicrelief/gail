using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace CharitiesOnlineWorkings
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

    }
}
