using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace CharitiesOnline.Builders
{
    public abstract class GovTalkDetailsBuilderBase
    {
        private hmrcclasses.GovTalkMessageGovTalkDetails _govTalkDetails;

        public hmrcclasses.GovTalkMessageGovTalkDetails GovTalkDetails
        {
            get
            {
                return _govTalkDetails;
            }
        }

        public void InitialiseGovTalkDetails()
        {
            _govTalkDetails = new hmrcclasses.GovTalkMessageGovTalkDetails();
        }
        public abstract void SetGovTalkDetailsKeys();
        public abstract void SetGovTalkDetailsTargetDetails();
        public abstract void SetGovTalkDetailsChannelRouting();
        public XmlElement SerializeGovTalkMessageGovTalkDetails(hmrcclasses.GovTalkMessageGovTalkDetails gtDetails)
        {
            // copy of IRenvelope serializer

            using (MemoryStream memStream = new MemoryStream())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(String.Empty, "http://www.govtalk.gov.uk/CM/envelope");

                var knownTypes = new Type[] { typeof(hmrcclasses.GovTalkMessageGovTalkDetails), typeof(hmrcclasses.GovTalkMessageGovTalkDetailsKey) };

                XmlSerializer serializer =
                    new XmlSerializer(typeof(hmrcclasses.GovTalkMessageGovTalkDetails), knownTypes);

                XmlTextWriter tw = new XmlTextWriter(memStream, UTF8Encoding.UTF8);

                XmlDocument doc = new XmlDocument();
                tw.Formatting = Formatting.Indented;
                tw.IndentChar = ' ';
                serializer.Serialize(tw, gtDetails, ns);
                memStream.Seek(0, SeekOrigin.Begin);
                doc.Load(memStream);
                XmlElement returnVal = doc.DocumentElement;

                return returnVal;
            }
        }

    }
    public class GovTalkDetailsCreator
    {
        private GovTalkDetailsBuilderBase _govTalkDetailsBuilder;

        public GovTalkDetailsCreator(GovTalkDetailsBuilderBase govTalkDetailsBuilder)
        {
            _govTalkDetailsBuilder = govTalkDetailsBuilder;
        }

        public void CreateGovTalkDetails()
        {
            _govTalkDetailsBuilder.InitialiseGovTalkDetails();
            _govTalkDetailsBuilder.SetGovTalkDetailsKeys();
            _govTalkDetailsBuilder.SetGovTalkDetailsTargetDetails();
            _govTalkDetailsBuilder.SetGovTalkDetailsChannelRouting();
        }


        public hmrcclasses.GovTalkMessageGovTalkDetails GetGovTalkDetails()
        {
            return _govTalkDetailsBuilder.GovTalkDetails;
        }

        public XmlElement SerializeGovTalkDetails(hmrcclasses.GovTalkMessageGovTalkDetails details)
        {
            return _govTalkDetailsBuilder.SerializeGovTalkMessageGovTalkDetails(details);
        }

    }
    public class SubmitRequestGovTalkDetailsBuilder : GovTalkDetailsBuilderBase
    {
        public void CreateGovTalkDetails()
        {
            InitialiseGovTalkDetails();
            SetGovTalkDetailsKeys();
            SetGovTalkDetailsTargetDetails();
            SetGovTalkDetailsChannelRouting();                
        }
        public override void SetGovTalkDetailsKeys()
        {
            // Key names need to be GovTakDetailsKey, GovTalkDetailsKey2, GovTalkDetaailsKeyType, GovTalkDetailsKey2Type, etc
            IEnumerable<hmrcclasses.GovTalkMessageGovTalkDetailsKey> Keys = ReferenceDataManager.Settings.AllKeys
                .Where(key => key.Contains("GovTalkDetailsKey") && !key.Contains("Type"))
                .Select(key => new hmrcclasses.GovTalkMessageGovTalkDetailsKey { Type = ReferenceDataManager.Settings[key + "Type"], Value = ReferenceDataManager.Settings[key] })
                .ToArray <hmrcclasses.GovTalkMessageGovTalkDetailsKey>();       
            
            GovTalkDetails.Keys = Keys.ToArray();
        }
        public override void SetGovTalkDetailsTargetDetails()
        {
            string[] TargetDetails = new string[1];
            TargetDetails[0] = ReferenceDataManager.Settings["GovTalkDetailsTargetOrganistion"];

            GovTalkDetails.TargetDetails = TargetDetails;

        }
        public override void SetGovTalkDetailsChannelRouting()
        {
            hmrcclasses.GovTalkMessageGovTalkDetailsChannelRoutingChannel Channel = new hmrcclasses.GovTalkMessageGovTalkDetailsChannelRoutingChannel();
            Channel.Version = ReferenceDataManager.Settings["ChannelVersion"];
            Channel.Product = ReferenceDataManager.Settings["ChannelProduct"];
            Channel.ItemElementName = hmrcclasses.ItemChoiceType.URI;
            Channel.Item = ReferenceDataManager.Settings["ChannelURI"];

            hmrcclasses.GovTalkMessageGovTalkDetailsChannelRouting ChannelRouting = new hmrcclasses.GovTalkMessageGovTalkDetailsChannelRouting();
            ChannelRouting.Channel = Channel;

            hmrcclasses.GovTalkMessageGovTalkDetailsChannelRouting[] ChannelRoutings =
                    new hmrcclasses.GovTalkMessageGovTalkDetailsChannelRouting[1];
            ChannelRoutings[0] = ChannelRouting;
            
            GovTalkDetails.ChannelRouting = ChannelRoutings;
        }
    }

    public class ListRequestGovTalkDetailsBuilder : GovTalkDetailsBuilderBase
    {
        public void CreateGovTalkDetails()
        {
            InitialiseGovTalkDetails();
            SetGovTalkDetailsKeys();
            SetGovTalkDetailsTargetDetails();
            SetGovTalkDetailsChannelRouting();  
        }
        public override void SetGovTalkDetailsKeys()
        {
            IEnumerable<hmrcclasses.GovTalkMessageGovTalkDetailsKey> Keys = ReferenceDataManager.Settings.AllKeys
                .Where(key => key.Contains("GovTalkDetailsKey") && !key.Contains("Type"))
                .Select(key => new hmrcclasses.GovTalkMessageGovTalkDetailsKey { Type = ReferenceDataManager.Settings[key + "Type"], Value = ReferenceDataManager.Settings[key] })
                .ToArray<hmrcclasses.GovTalkMessageGovTalkDetailsKey>();

            GovTalkDetails.Keys = Keys.ToArray();
        }
        public override void SetGovTalkDetailsTargetDetails() {}
        public override void SetGovTalkDetailsChannelRouting() {}            
        
    }
}
