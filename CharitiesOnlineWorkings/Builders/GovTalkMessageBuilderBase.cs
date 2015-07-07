using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using System.IO;


using hmrcclasses;


namespace CharitiesOnlineWorkings.Builders
{
    public abstract class GovTalkMessageBuilderBase
    {
        private hmrcclasses.GovTalkMessage _govTalkMessage;
        
        private string _correlationId;
        public string CorrelationId
        {
            get
            {
                return _correlationId;
            }
            set
            {
                _correlationId = value;
            }
        }
        public hmrcclasses.GovTalkMessage GovTalkMessage
        {
            get
            {
                return _govTalkMessage;
            }
        }
        public void InitialiseGovTalkMessage()
        {
            _govTalkMessage = new hmrcclasses.GovTalkMessage();
        }

        public abstract void SetEnvelopeVersion();
        public abstract void SetHeader();
        public abstract void SetGovTalkDetails();
        public abstract void SetBody();
    }

    public class GovTalkMessageCreator
    {
        private GovTalkMessageBuilderBase _govTalkMessageBuilder;
        public GovTalkMessageCreator(GovTalkMessageBuilderBase govTalkMessageBuilder)
        {
            _govTalkMessageBuilder = govTalkMessageBuilder;
        }

        public void CreateGovTalkMessage()
        {
            _govTalkMessageBuilder.InitialiseGovTalkMessage();
            _govTalkMessageBuilder.SetEnvelopeVersion();
            _govTalkMessageBuilder.SetHeader();
            _govTalkMessageBuilder.SetGovTalkDetails();
            _govTalkMessageBuilder.SetBody();
        }

        public hmrcclasses.GovTalkMessage GetGovTalkMessage()
        {
            return _govTalkMessageBuilder.GovTalkMessage;
        }

        public XmlDocument SerializeGovTalkMessage()
        {
            using(MemoryStream memStream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "\t",
                    Encoding = Encoding.UTF8
                };

                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, "http://www.govtalk.gov.uk/CM/envelope");

                XmlSerializer serializer = new XmlSerializer(typeof(hmrcclasses.GovTalkMessage));
                serializer.Serialize(XmlWriter.Create(memStream,settings), _govTalkMessageBuilder.GovTalkMessage, ns);
                
                memStream.Seek(0, SeekOrigin.Begin);

                // Use XmlWriter to make use of settings
                // Load to XmlDoc for ease of use by client               

                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true; 
                doc.Load(memStream);
                
                XmlDeclaration xmlDeclaration;
                xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

                doc.ReplaceChild(xmlDeclaration, doc.FirstChild);

                return doc;
            }            
        }
       


        public void SetCorrelationId(string correlationId)
        {
            _govTalkMessageBuilder.CorrelationId = correlationId;
        }

        
    }

    public class DefaultGovTalkMessageBuilder : GovTalkMessageBuilderBase
    {
        public void CreateGovTalkMessage()
        {
            InitialiseGovTalkMessage();
            SetEnvelopeVersion();
            SetHeader();
            SetGovTalkDetails();
            SetBody();
        }

        public override void SetEnvelopeVersion()
        {
            GovTalkMessage.EnvelopeVersion = ReferenceDataManager.Settings["GovTalkMessageEnvelopeVersion"];
        }
        public override void SetHeader()
        {
            throw new NotImplementedException();
        }
        public override void SetGovTalkDetails()
        {
            throw new NotImplementedException();
        }
        public override void SetBody()
        {
            throw new NotImplementedException();
        }
    }

    public class SubmitRequestMessageBuilder : DefaultGovTalkMessageBuilder
    {
        public override void SetHeader()
        {
            // get values from config
            HeaderCreator submitRequestHeaderCreator = new HeaderCreator(new RequestHeaderBuilder());
            submitRequestHeaderCreator.CreateHeader();
            GovTalkMessage.Header = submitRequestHeaderCreator.GetHeader();
        }
        public override void SetGovTalkDetails()
        {
            // get values from config
            GovTalkDetailsCreator govTalkDetailsCreatoor = new GovTalkDetailsCreator(new SubmitRequestGovTalkDetailsBuilder());
            govTalkDetailsCreatoor.CreateGovTalkDetails();
            GovTalkMessage.GovTalkDetails = govTalkDetailsCreatoor.GetGovTalkDetails();
        }

        public override void SetBody()
        {
            BodyCreator bodyCreator = new BodyCreator(new SubmitRequestBodyBuilder());
            bodyCreator.CreateBody();
            GovTalkMessage.Body = bodyCreator.GetBody();
        }
    }

    public class SubmitRequestCompressedMessageBuilder : DefaultGovTalkMessageBuilder
    {
        public override void SetHeader()
        {
            // get values from config
            HeaderCreator submitRequestHeaderCreator = new HeaderCreator(new RequestHeaderBuilder());
            submitRequestHeaderCreator.CreateHeader();
            GovTalkMessage.Header = submitRequestHeaderCreator.GetHeader();
        }
        public override void SetGovTalkDetails()
        {
            // get values from config
            GovTalkDetailsCreator govTalkDetailsCreatoor = new GovTalkDetailsCreator(new SubmitRequestGovTalkDetailsBuilder());
            govTalkDetailsCreatoor.CreateGovTalkDetails();
            GovTalkMessage.GovTalkDetails = govTalkDetailsCreatoor.GetGovTalkDetails();
        }

        public override void SetBody()
        {
            BodyCreator bodyCreator = new BodyCreator(new SubmitRequestCompressedBodyBuilder());
            bodyCreator.CreateBody();
            GovTalkMessage.Body = bodyCreator.GetBody();
        }
    }

    public class SubmitPollMesageBuilder : DefaultGovTalkMessageBuilder
    {
        
        public override void SetHeader()
        {
            PollHeaderBuilder pollHeaderBuilder = new PollHeaderBuilder();
            pollHeaderBuilder.CorrelationId = CorrelationId;

            HeaderCreator pollHeaderCreator = new HeaderCreator(pollHeaderBuilder);
            pollHeaderCreator.CreateHeader();

            GovTalkMessage.Header = pollHeaderCreator.GetHeader();
            
        }

        public override void SetGovTalkDetails()
        {
            GovTalkMessageGovTalkDetails govTalkDetails = new GovTalkMessageGovTalkDetails();
            GovTalkMessageGovTalkDetailsKey[] Keys = new GovTalkMessageGovTalkDetailsKey[1];
            govTalkDetails.Keys = Keys;
            GovTalkMessage.GovTalkDetails = govTalkDetails;
        }
        
        public override void SetBody()
        {
            BodyCreator pollBodyCreator = new BodyCreator(new EmptyBodyBuilder());
            pollBodyCreator.CreateBody();

            GovTalkMessage.Body = pollBodyCreator.GetBody();
        }        
    }

    public class DeleteRequestMessageBuilder : DefaultGovTalkMessageBuilder
    {
        public override void SetHeader()
        {
            DeleteHeaderBuilder deleteHeaderBuilder = new DeleteHeaderBuilder();
            deleteHeaderBuilder.CorrelationId = CorrelationId;
            HeaderCreator deleteHeaderCreator = new HeaderCreator(deleteHeaderBuilder);
            deleteHeaderCreator.CreateHeader();

            GovTalkMessage.Header = deleteHeaderCreator.GetHeader();
        }

        public override void SetGovTalkDetails()
        {
            GovTalkMessageGovTalkDetails govTalkDetails = new GovTalkMessageGovTalkDetails();
            GovTalkMessageGovTalkDetailsKey[] Keys = new GovTalkMessageGovTalkDetailsKey[1];
            govTalkDetails.Keys = Keys;
            GovTalkMessage.GovTalkDetails = govTalkDetails;
        }

        public override void SetBody()
        {
            BodyCreator deleteBodyCreator = new BodyCreator(new EmptyBodyBuilder());
            deleteBodyCreator.CreateBody();

            GovTalkMessage.Body = deleteBodyCreator.GetBody();
        }
    }


    // @TODO: ADDPASSWORD properly ...
    public class ListRequestMessageBuilder : DefaultGovTalkMessageBuilder
    {
        public override void SetHeader()
        {
            HeaderCreator listRequestHeaderCreator = new HeaderCreator(new ListRequestHeaderBuilder());
            listRequestHeaderCreator.CreateHeader();

            GovTalkMessage.Header = listRequestHeaderCreator.GetHeader();
        }

        public override void SetGovTalkDetails()
        {
            GovTalkDetailsCreator govTalkDetailsCreator = new GovTalkDetailsCreator(new ListRequestGovTalkDetailsBuilder());
            govTalkDetailsCreator.CreateGovTalkDetails();
            GovTalkMessage.GovTalkDetails = govTalkDetailsCreator.GetGovTalkDetails();
        }

        public override void SetBody()
        {
            BodyCreator listRequestBodyCreator = new BodyCreator(new EmptyBodyBuilder());
            listRequestBodyCreator.CreateBody();

            GovTalkMessage.Body = listRequestBodyCreator.GetBody();
        }

    }

}
