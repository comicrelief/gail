using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

using System.IO.Compression;

using System.Xml;
using System.Xml.Serialization;
using hmrcclasses;

using System.Xml.Linq;

using System.Reflection;

using LumenWorks.Framework.IO.Csv;

using CharitiesOnlineWorkings.XmlWriterExtension;

namespace CharitiesOnlineWorkings
{
    public static class Helpers
    {
        public static DataTable GetDataTableFromCsv(string path, bool isFirstRowHeader)
        {
            //Uses CsvReader from http://www.codeproject.com/Articles/9258/A-Fast-CSV-Reader
            try
            {
                DataTable dataTable = new DataTable();

                using (CsvReader csv =
                    new CsvReader(new StreamReader(path), true))
                {

                    dataTable.Load(csv);
                }

                return dataTable;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

        }

        public static XmlElement SerializeIREnvelope(IRenvelope ire)
        {
            try
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
            catch (Exception ex)
            {
                throw;
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

        //@TODO: version that takes an XmlElement
        public static XmlDocument SetIRmark(XmlDocument XmlFile)
        {
            // Loads XML document into byte array           
            byte[] bytes = Encoding.UTF8.GetBytes(XmlFile.OuterXml);

            string vbLf = "\n";
            string vbCrLf = "\r\n";
            string result = String.Empty;

            string text = Encoding.UTF8.GetString(bytes);

            XmlNode root = XmlFile.DocumentElement;

            XmlNamespaceManager nsManager = new XmlNamespaceManager(XmlFile.NameTable);
            nsManager.AddNamespace("govtalkgateway", XmlFile.DocumentElement.NamespaceURI);

            // Create an XML document of just the Body element
            XmlNode bodyNode = XmlFile.SelectSingleNode("//govtalkgateway:Body", nsManager);

            //nsManager.AddNamespace("ir68", bodyNode.FirstChild.NextSibling.NamespaceURI);
            nsManager.AddNamespace("ir68", "http://www.govtalk.gov.uk/taxation/charities/r68/2");

            XmlDocument xmlBody = new XmlDocument();
            xmlBody.PreserveWhitespace = true;
            xmlBody.LoadXml(bodyNode.OuterXml);

            // Remove any existing IRmark
            XmlNode nodeIr = xmlBody.SelectSingleNode("//ir68:IRmark", nsManager);
            XmlNode irMarkPlaceholder = nodeIr;
            if(nodeIr != null)
            {
                irMarkPlaceholder = nodeIr.PreviousSibling;
                nodeIr.ParentNode.RemoveChild(nodeIr);
            }

            // Normalise the document using C14N (Canonicalisation)
            System.Security.Cryptography.Xml.XmlDsigC14NTransform c14n = new System.Security.Cryptography.Xml.XmlDsigC14NTransform();
            c14n.LoadInput(xmlBody);

            using (Stream stream = (Stream)c14n.GetOutput())
            {
                byte[] buffer = new byte[stream.Length];

                // convert to string and normalise line endings
                stream.Read(buffer, 0, (int)stream.Length);
                text = Encoding.UTF8.GetString(buffer);
                text = text.Replace("&#xD;", "");
                text = text.Replace(vbCrLf, vbLf);
                
                // convert the final document back into a byte array
                byte[] textBytes = Encoding.UTF8.GetBytes(text);

                //create the SHA-1 hash from the final document
                System.Security.Cryptography.SHA1 sha = System.Security.Cryptography.SHA1.Create();
                byte[] hash = sha.ComputeHash(textBytes);

                result = Convert.ToBase64String(hash);
            }

            // attempt to re-insert the IRmark

            XmlNode irMarkNode = root.SelectSingleNode("//*[contains(name(),'IRmark')]");

            if(!String.IsNullOrEmpty(irMarkNode.InnerText))
            {
                root.SelectSingleNode("//*[contains(name(),'IRmark')]").LastChild.Value = result;
            }
            else
            {
                if(root.SelectSingleNode("//*[contains(name(),'IRmark')]") != null)
                {
                    irMarkNode.InnerText = result;
                }
                else
                {
                    Console.WriteLine("No IRmark");
                }
            }
            return XmlFile;
        }
        public static byte[] CompressData(string inputXml)
        {
            var bytes = Encoding.UTF8.GetBytes(inputXml);

            byte[] compressedBytes;

            using(var compressedStream = new MemoryStream())
            using(var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(bytes, 0, bytes.Length);
                zipStream.Close();

                compressedBytes = compressedStream.ToArray();
            }

            return compressedBytes;
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

        public static string DecompressData(byte[] inputXml)
        {
            //var bytes = Encoding.UTF8.GetBytes(inputXml);

            using(var msi = new MemoryStream(inputXml))
            using(var mso = new MemoryStream())
            {
                using(var gs = new GZipStream(msi,CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
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

        // Is this a candidate for the SubmitRequest message reader?
        public static XmlDocument GetClaim(XmlDocument r68)
        {
            //receive an XmlDocument containing a serialized R68, return an XmlDocument containing a Claim

            XmlNode claimNode = r68.SelectSingleNode("R68/Claim");

            XmlDocument claim = new XmlDocument();
            
            claim.LoadXml(claimNode.OuterXml);

            return claim;
            
        }

        public static DataTable MakeRepaymentTable()
        {
            DataTable dt = new DataTable("RepaymentDonors");

            DataColumn Fore = new DataColumn();
            Fore.DataType = System.Type.GetType("System.String");
            Fore.ColumnName = "Fore";
            dt.Columns.Add(Fore);

            DataColumn Sur = new DataColumn();
            Sur.DataType = System.Type.GetType("System.String");
            Sur.ColumnName = "Sur";
            dt.Columns.Add(Sur);

            DataColumn House = new DataColumn();
            House.DataType = System.Type.GetType("System.String");
            House.ColumnName = "House";
            dt.Columns.Add(House);

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.String"),
                ColumnName = "Postcode"
            });

            dt.Columns.Add(new DataColumn
                {
                    DataType = System.Type.GetType("System.Decimal"),
                    ColumnName = "Total"
                });

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.Datetime"),
                ColumnName = "Date"
            });

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.String"),
                ColumnName = "Type"
            });

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.String"),
                ColumnName = "Overseas"
            });

            dt.Columns.Add(new DataColumn
                {
                    DataType = System.Type.GetType("System.String"),
                    ColumnName = "Description"
                });

            return dt;

        }

        public static DataTable MakeOtherIncomeTable()
        {
            DataTable dt = new DataTable("OtherInc");

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.String"),
                ColumnName = "Payer"
            });

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.DateTime"),
                ColumnName = "OIDate"
            });

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.Decimal"),
                ColumnName = "Gross"
            });

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.Decimal"),
                ColumnName = "Tax"
            });

            return dt;
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
