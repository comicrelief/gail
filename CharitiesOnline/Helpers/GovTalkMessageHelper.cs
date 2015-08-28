using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;

using CR.Infrastructure.Configuration;
using CR.Infrastructure.Logging;
using hmrcclasses;

namespace CharitiesOnline.Helpers
{
    /// <summary>
    /// Useful methods for working with GovTalkMessage objects and serialized representations.
    /// </summary>
    public class GovTalkMessageHelper
    {
        private IConfigurationRepository _configurationRepository;
        private ILoggingService _loggingService;

        public GovTalkMessageHelper(IConfigurationRepository configurationRepository, ILoggingService loggingService)
        {
            _configurationRepository = configurationRepository;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Set the IRmark for a 
        /// </summary>
        /// <param name="XmlFile"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Add a password to a serialized GovTalkMessage
        /// </summary>
        /// <param name="inputXDocument"></param>
        /// <param name="userPassword"></param>
        /// <param name="passwordMethod"></param>
        /// <returns></returns>
        public XDocument AddPassword( XDocument inputXDocument, string userPassword, string passwordMethod = "")
        {
            _loggingService.LogInfo(this, "Adding input password via Xml.Linq");

            bool MD5Password = false;

            if ((passwordMethod == "MD5" || _configurationRepository.GetConfigurationValue<string>("SenderAuthenticationMethod") == "MD5") && passwordMethod.ToLower() != "clear")
            {
                MD5Password = true;
            }
            else
            {
                MD5Password = false;
            }

            if(MD5Password)
            {
                CommonUtilityHelper helper = new CommonUtilityHelper(_configurationRepository, _loggingService);
                userPassword = helper.MD5Hash(userPassword);
            }

            XElement root = XElement.Parse(inputXDocument.ToString());
            XNamespace GovTalk = "http://www.govtalk.gov.uk/CM/envelope";

            //The name of the password element is 'value'. 
            // The full path is /GovTalkMessage/Header/SenderDetails/IDAuthentication/Authentication/Value .

            IEnumerable<XElement> PasswordTree =
                    (from el in root.Descendants(GovTalk + "Value")
                     select el);
            if (PasswordTree.Count() == 0 || PasswordTree.ElementAt(0).Name.LocalName != "Value")
            {
                _loggingService.LogWarning(this, "Password element not found.");
                return inputXDocument;
            }

            XElement Password = PasswordTree.ElementAt(0);
            XElement NewPassword = new XElement(GovTalk + "Value", userPassword);
            Password.ReplaceWith(NewPassword);

            IEnumerable<XElement> MethodTree =
                    (from el in root.Descendants(GovTalk + "Method")
                     select el);
            XElement PassMethod = MethodTree.ElementAt(0);
            XElement NewMethod = MD5Password ? new XElement(GovTalk + "Method", "MD5") : new XElement(GovTalk + "Method", "clear");
            PassMethod.ReplaceWith(NewMethod);

            XDocument outputXDocument = new XDocument(root);
            _loggingService.LogInfo(this, "Password added to XDocument.");

            return outputXDocument;
        }

        /// <summary>
        /// Set a password within a GovTalkMessage object
        /// </summary>
        /// <param name="govTalkMessage"></param>
        /// <param name="userPassword"></param>
        /// <param name="passwordMethod"></param>
        public void SetPassword(GovTalkMessage govTalkMessage, string userPassword, string passwordMethod = "")
        {
            _loggingService.LogInfo(this, "Setting password.");

            if((passwordMethod == "MD5" || _configurationRepository.GetConfigurationValue<string>("SenderAuthenticationMethod") == "MD5") && passwordMethod.ToLower() != "clear")
            {
                CommonUtilityHelper helper = new CommonUtilityHelper(_configurationRepository,_loggingService);

                govTalkMessage.Header.SenderDetails.IDAuthentication.Authentication[0].Method = GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthenticationMethod.MD5;

                userPassword = helper.MD5Hash(userPassword);

                _loggingService.LogDebug(this, "MD5 Hashed password.");
            }
            if(passwordMethod.ToLower() == "clear")
            {
                govTalkMessage.Header.SenderDetails.IDAuthentication.Authentication[0].Method = GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthenticationMethod.clear;
            }

            govTalkMessage.Header.SenderDetails.IDAuthentication.Authentication[0].Item = userPassword;

            _loggingService.LogInfo(this, "Password set.");
        }

        public static string MessageQualifier(GovTalkMessageHeaderMessageDetailsQualifier qualifier)
        {
            return qualifier.ToString();
        }
    }
}
