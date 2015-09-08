using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

using System.Xml;
using System.Xml.Serialization;

using hmrcclasses;

using System.Xml.Linq;

using System.Reflection;

using CharitiesOnline.XmlWriterExtension;

namespace CharitiesOnline.Helpers
{
    public static class XmlSerializationHelpers
    {      
        public static XmlDocument SerializeGovTalkMessage(GovTalkMessage govTalkMessage)
        {
            // Use XmlWriter to make use of settings
            // Load to XmlDoc for ease of use by client    

            using (MemoryStream memStream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "\t",
                    Encoding = Encoding.UTF8
                };

                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, "http://www.govtalk.gov.uk/CM/envelope");

                XmlSerializer serializer = new XmlSerializer(typeof(GovTalkMessage));
                serializer.Serialize(XmlWriter.Create(memStream, settings), govTalkMessage, ns);

                memStream.Seek(0, SeekOrigin.Begin);          

                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.Load(memStream);

                XmlDeclaration xmlDeclaration;
                xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

                doc.ReplaceChild(xmlDeclaration, doc.FirstChild);

                return doc;
            }            
        }
        public static XmlElement SerializeIREnvelope(IRenvelope ire)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(String.Empty, "http://www.govtalk.gov.uk/taxation/charities/r68/2");

                var knownTypes = new Type[] { typeof(IRenvelope), typeof(R68Claim) };

                XmlSerializer serializer =
                    new XmlSerializer(typeof(IRenvelope), knownTypes);

                XmlTextWriter tw = new XmlTextWriter(memStream, UTF8Encoding.UTF8);

                XmlDocument doc = new XmlDocument();
                tw.Formatting = Formatting.Indented;
                // tw.IndentChar = '\x09'; //tab
                tw.IndentChar = ' ';
                serializer.Serialize(tw, ire, ns);
                memStream.Seek(0, SeekOrigin.Begin);
                doc.Load(memStream);
                XmlElement returnVal = doc.DocumentElement;

                return returnVal;
            }
        }

        public static IRenvelope DeserializeIRenvelope(XmlDocument xmlElement)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(IRenvelope));
            MemoryStream xmlStream = new MemoryStream();
            xmlElement.Save(xmlStream);
            xmlStream.Seek(0, SeekOrigin.Begin);

            var LocalIRenvelope = serializer.Deserialize(xmlStream);
            IRenvelope ire = (IRenvelope)LocalIRenvelope;

            return ire;            
        }

        public static SuccessResponse DeserializeSuccessResponse(XmlDocument xmlElement)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SuccessResponse));
            MemoryStream xmlStream = new MemoryStream();
            xmlElement.Save(xmlStream);
            xmlStream.Seek(0, SeekOrigin.Begin);

            var LocalSuccessResponse = serializer.Deserialize(xmlStream);
            SuccessResponse success = (SuccessResponse)LocalSuccessResponse;

            return success;
        }

        public static GovTalkMessageBodyStatusReport DeserializeStatusReport(XmlDocument xmlElement)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GovTalkMessageBodyStatusReport));
            MemoryStream xmlStream = new MemoryStream();
            xmlElement.Save(xmlStream);
            xmlStream.Seek(0, SeekOrigin.Begin);

            var LocalStatusReport = serializer.Deserialize(xmlStream);
            GovTalkMessageBodyStatusReport statusReport = (GovTalkMessageBodyStatusReport)LocalStatusReport;

            return statusReport;
        }

        public static ErrorResponse DeserializeErrorResponse(XmlDocument xmlElement)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ErrorResponse));
            MemoryStream xmlStream = new MemoryStream();
            xmlElement.Save(xmlStream);
            xmlStream.Seek(0, SeekOrigin.Begin);

            var LocalErrorResponse = serializer.Deserialize(xmlStream);
            ErrorResponse error = (ErrorResponse)LocalErrorResponse;

            return error;
        }

        public static R68Claim DeserializeR68Claim(XmlDocument xmlElement)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(R68Claim));
            MemoryStream xmlStream = new MemoryStream();
            xmlElement.Save(xmlStream);
            xmlStream.Seek(0, SeekOrigin.Begin);

            var LocalR68Claim = serializer.Deserialize(xmlStream);
            R68Claim r68claim = (R68Claim)LocalR68Claim;

            return r68claim;
        }

        public static XmlDocument SerializeItem(object Item)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(String.Empty, "");
                ns.Add(String.Empty, "http://www.govtalk.gov.uk/taxation/charities/r68/2");

                var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };

                // , typeof(IRenvelope), typeof(R68Claim), typeof(R68CompressedPart)

                var knownTypes = new Type[] { typeof(hmrcclasses.R68) };

                using (XmlWriter innerWriter = XmlWriter.Create(memStream, settings))
                using (XmlWriter writer = new NamespaceSupressingXmlWriter(innerWriter))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.PreserveWhitespace = true;

                    XmlSerializer serializer =
                        new XmlSerializer(Item.GetType(), knownTypes);
                    serializer.Serialize(writer, Item, ns);
                    memStream.Seek(0, SeekOrigin.Begin);
                    
                    doc.Load(memStream);

                    return doc;
                }
            }
        }

        public static void SerializeToFile(hmrcclasses.GovTalkMessage gtMsg, string outputFile)
        {
            string filename = outputFile;

            using(StreamWriter output =
                new StreamWriter(new FileStream(outputFile,FileMode.Create), Encoding.UTF8))
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                // Need to change the namespace declarations in the GovTalkMessage attributes
                ns.Add(String.Empty, "http://www.govtalk.gov.uk/CM/envelope");

                XmlSerializer serializer =
                            new XmlSerializer(typeof(hmrcclasses.GovTalkMessage));
                serializer.Serialize(output, gtMsg, ns);
            }
        }

        public static GovTalkMessage DeserializeMessage(XmlDocument XmlMessage)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GovTalkMessage));
            MemoryStream xmlStream = new MemoryStream();

            XmlMessage.Save(xmlStream);
            xmlStream.Seek(0, SeekOrigin.Begin);

            var LocalGovTalkMsg = serializer.Deserialize(xmlStream);
            GovTalkMessage reply = (GovTalkMessage)LocalGovTalkMsg;

            return reply;
        }        

        #region Alternative
        // from http://stackoverflow.com/questions/258960/how-to-serialize-an-object-to-xml-without-getting-xmlns

        private static readonly XmlWriterSettings WriterSettings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };
        private static readonly XmlSerializerNamespaces Namespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") });

        public static string Serialize(object obj)
        {
            if (obj == null)
                return null;

            return DoSerialize(obj);
        }

        private static string DoSerialize(object obj)
        {
            using(var ms = new MemoryStream())
            using(var writer = XmlWriter.Create(ms, WriterSettings))
            {
                var serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(writer, obj, Namespaces);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static T Deserialize<T>(string data, string rootName) where T : class
        {
            if(string.IsNullOrEmpty(data))
            {
                return null;
            }
            return DoDeserialize<T>(data, rootName);
        }

        private static T DoDeserialize<T>(string data, string rootName) where T : class
        {
            using(var ms = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                // OK, this is a bit weird, coupling this to the particular namespace
                // It's a bodge

                XmlRootAttribute xRoot = new XmlRootAttribute();
                xRoot.ElementName = rootName;
                xRoot.Namespace = "http://www.govtalk.gov.uk/taxation/charities/r68/2";
                xRoot.IsNullable = false;

                ms.Seek(0, SeekOrigin.Begin);
                var serializer = new XmlSerializer(typeof(T),xRoot);
                return (T)serializer.Deserialize(ms);
            }
        }

        #endregion Alternative

    }
}
