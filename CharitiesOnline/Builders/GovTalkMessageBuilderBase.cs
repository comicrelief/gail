using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

using hmrcclasses;
using CR.Infrastructure.Logging;
using CharitiesOnline.Helpers;

namespace CharitiesOnline.Builders
{
    public abstract class GovTalkMessageBuilderBase
    {
        private GovTalkMessage _govTalkMessage;  
        private string _correlationId;
        private ILoggingService _loggingService;

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
        public GovTalkMessage GovTalkMessage
        {
            get
            {
                return _govTalkMessage;
            }
        }
        public void InitialiseGovTalkMessage(ILoggingService loggingService)
        {
            _govTalkMessage = new GovTalkMessage();
            _loggingService = loggingService;

            loggingService.LogInfo(this, "Initialised GovTalkMessage.");

        }

        public abstract void SetEnvelopeVersion();
        public abstract void SetHeader();
        public abstract void SetGovTalkDetails();
        public abstract void SetBody();
    }

    public class GovTalkMessageCreator
    {
        private GovTalkMessageBuilderBase _govTalkMessageBuilder;
        private ILoggingService _loggingService;
        public GovTalkMessageCreator(GovTalkMessageBuilderBase govTalkMessageBuilder, ILoggingService loggingService)
        {
            _govTalkMessageBuilder = govTalkMessageBuilder;
            _loggingService = loggingService;
        }

        public void CreateGovTalkMessage()
        {
            _govTalkMessageBuilder.InitialiseGovTalkMessage(_loggingService);
            _govTalkMessageBuilder.SetEnvelopeVersion();
            _govTalkMessageBuilder.SetHeader();
            _govTalkMessageBuilder.SetGovTalkDetails();
            _govTalkMessageBuilder.SetBody();
        }

        public GovTalkMessage GetGovTalkMessage()
        {
            return _govTalkMessageBuilder.GovTalkMessage;
        }

        public XmlDocument SerializeGovTalkMessage()
        {
            _loggingService.LogInfo(this, "Serializing GovTalkMessage.");

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

                XmlSerializer serializer = new XmlSerializer(typeof(GovTalkMessage));
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

        public XmlDocument CompressClaim()
        {
            _loggingService.LogInfo(this, "Compressing Claim.");

            XmlElement bodyElement = _govTalkMessageBuilder.GovTalkMessage.Body.Any[0];
            XmlDocument bodyDocument = new XmlDocument();
            bodyDocument.LoadXml(bodyElement.OuterXml);

            //deserialize body
            IRenvelope irEnvelope = XmlSerializationHelpers.DeserializeIRenvelope(bodyDocument);

            R68 uncompressedR68 = irEnvelope.R68;
            XmlDocument r68xmlDoc = XmlSerializationHelpers.SerializeItem(uncompressedR68);

            System.Xml.XmlDocument claimXmlDoc = GovTalkMessageHelpers.GetClaim(r68xmlDoc);

            irEnvelope.R68.Items = null;

            R68CompressedPart compressedPart = new R68CompressedPart();
            compressedPart.Type = R68CompressedPartType.gzip;
            compressedPart.Value = CommonUtilityHelpers.CompressData(claimXmlDoc.OuterXml, _loggingService);

            R68CompressedPart[] compressedParts = new R68CompressedPart[1];
            compressedParts[0] = compressedPart;

            irEnvelope.R68.Items = compressedParts;

            bodyElement = XmlSerializationHelpers.SerializeIREnvelope(irEnvelope);

            _govTalkMessageBuilder.GovTalkMessage.Body.Any[0] = null;
            _govTalkMessageBuilder.GovTalkMessage.Body.Any[0] = bodyElement;

            XmlDocument compressedVersion = SerializeGovTalkMessage();

            return compressedVersion;
        }

        public void SetCorrelationId(string correlationId)
        {
            _govTalkMessageBuilder.CorrelationId = correlationId;
        }        
    }

    public class DefaultGovTalkMessageBuilder : GovTalkMessageBuilderBase
    {
        public DefaultGovTalkMessageBuilder(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        private ILoggingService _loggingService;
        public void CreateGovTalkMessage()
        {
            InitialiseGovTalkMessage(_loggingService);
            SetEnvelopeVersion();
            SetHeader();
            SetGovTalkDetails();
            SetBody();
        }

        public override void SetEnvelopeVersion()
        {            
            GovTalkMessage.EnvelopeVersion = ReferenceDataManager.Settings["GovTalkMessageEnvelopeVersion"];
            _loggingService.LogInfo(this, "Envelope Version set");
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
        private ILoggingService _loggingService;
        public SubmitRequestMessageBuilder(ILoggingService loggingService) : base(loggingService)
        {
            _loggingService = loggingService;
        }
        public override void SetHeader()
        {
            // get values from config
            HeaderCreator submitRequestHeaderCreator = new HeaderCreator(new RequestHeaderBuilder(_loggingService), _loggingService);
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
            BodyCreator bodyCreator = new BodyCreator(new SubmitRequestBodyBuilder(_loggingService), _loggingService);
            bodyCreator.CreateBody();
            GovTalkMessage.Body = bodyCreator.GetBody();
        }       
    }

    public class SubmitRequestCompressedMessageBuilder : DefaultGovTalkMessageBuilder
    {
        private ILoggingService _loggingService;

        public SubmitRequestCompressedMessageBuilder(ILoggingService loggingService) : base(loggingService)
        {
            _loggingService = loggingService;
        }
        
        public override void SetHeader()
        {
            // get values from config
            HeaderCreator submitRequestHeaderCreator = new HeaderCreator(new RequestHeaderBuilder(_loggingService),_loggingService);
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
            BodyCreator bodyCreator = new BodyCreator(new SubmitRequestCompressedBodyBuilder(_loggingService), _loggingService);
            bodyCreator.CreateBody();
            GovTalkMessage.Body = bodyCreator.GetBody();
        }
    }

    public class SubmitPollMesageBuilder : DefaultGovTalkMessageBuilder
    {
        private ILoggingService _loggingService;

        public SubmitPollMesageBuilder(ILoggingService loggingService) : base(loggingService)
        {
            _loggingService = loggingService;
        }
        public override void SetHeader()
        {
            PollHeaderBuilder pollHeaderBuilder = new PollHeaderBuilder(_loggingService);
            pollHeaderBuilder.CorrelationId = CorrelationId;

            HeaderCreator pollHeaderCreator = new HeaderCreator(pollHeaderBuilder, _loggingService);
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
            BodyCreator pollBodyCreator = new BodyCreator(new EmptyBodyBuilder(_loggingService), _loggingService);
            pollBodyCreator.CreateBody();

            GovTalkMessage.Body = pollBodyCreator.GetBody();
        }        
    }

    public class DeleteRequestMessageBuilder : DefaultGovTalkMessageBuilder
    {
        private ILoggingService _loggingService;

        public DeleteRequestMessageBuilder(ILoggingService loggingService) : base(loggingService)
        {
            _loggingService = loggingService;
        }
        public override void SetHeader()
        {
            DeleteHeaderBuilder deleteHeaderBuilder = new DeleteHeaderBuilder(_loggingService);
            deleteHeaderBuilder.CorrelationId = CorrelationId;
            HeaderCreator deleteHeaderCreator = new HeaderCreator(deleteHeaderBuilder, _loggingService);
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
            BodyCreator deleteBodyCreator = new BodyCreator(new EmptyBodyBuilder(_loggingService), _loggingService);
            deleteBodyCreator.CreateBody();

            GovTalkMessage.Body = deleteBodyCreator.GetBody();
        }
    }

    // @TODO: ADDPASSWORD properly ...
    public class ListRequestMessageBuilder : DefaultGovTalkMessageBuilder
    {
        private ILoggingService _loggingService;

        public ListRequestMessageBuilder(ILoggingService loggingService) : base(loggingService)
        {
            _loggingService = loggingService;
        }
        public override void SetHeader()
        {
            HeaderCreator listRequestHeaderCreator = new HeaderCreator(new ListRequestHeaderBuilder(_loggingService), _loggingService);
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
            BodyCreator listRequestBodyCreator = new BodyCreator(new EmptyBodyBuilder(_loggingService), _loggingService);
            listRequestBodyCreator.CreateBody();

            GovTalkMessage.Body = listRequestBodyCreator.GetBody();
        }

    }

}
