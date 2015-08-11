using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.IO;

using hmrcclasses;

namespace CharitiesOnline.Helpers
{
    public class GovTalkMessageHelpers
    {
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
            if (nodeIr != null)
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

            if (!String.IsNullOrEmpty(irMarkNode.InnerText))
            {
                root.SelectSingleNode("//*[contains(name(),'IRmark')]").LastChild.Value = result;
            }
            else
            {
                if (root.SelectSingleNode("//*[contains(name(),'IRmark')]") != null)
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

        // Is this a candidate for the SubmitRequest message reader?
        public static XmlDocument GetClaim(XmlDocument r68)
        {
            //receive an XmlDocument containing a serialized R68, return an XmlDocument containing a Claim

            XmlNode claimNode = r68.SelectSingleNode("R68/Claim");

            XmlDocument claim = new XmlDocument();

            claim.LoadXml(claimNode.OuterXml);

            return claim;
        }

        public static string MessageQualifier(GovTalkMessageHeaderMessageDetailsQualifier qualifier)
        {
            return qualifier.ToString();
        }
    }
}
