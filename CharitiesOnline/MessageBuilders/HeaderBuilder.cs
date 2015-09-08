using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using hmrcclasses;
using CR.Infrastructure.Logging;

namespace CharitiesOnline.MessageBuilders
{
    public abstract class HeaderBuilderBase
    {
        private GovTalkMessageHeader _header;
        private ILoggingService _loggingService;

        public GovTalkMessageHeader Header
        {
            get
            {
                return _header;
            }
        }

        public void InitialiseHeader(ILoggingService loggingService)
        {
            _header = new GovTalkMessageHeader();
            _loggingService = loggingService;
        }

        public abstract void SetMessageDetails();
        public abstract void SetSenderDetails();
    }

    public class HeaderCreator
    {
        private HeaderBuilderBase _headerBuilder;
        private ILoggingService _loggingService;

        public HeaderCreator(HeaderBuilderBase headerBuilder, ILoggingService loggingService)
        {
            _headerBuilder = headerBuilder;
            _loggingService = loggingService;
        }

        public void CreateHeader()
        {
            _headerBuilder.InitialiseHeader(_loggingService);
            _headerBuilder.SetMessageDetails();
            _headerBuilder.SetSenderDetails();
        }

        public GovTalkMessageHeader GetHeader()
        {
            return _headerBuilder.Header;
        }
    }

    public class RequestHeaderBuilder : HeaderBuilderBase
    {
        private ILoggingService _loggingService;

        public RequestHeaderBuilder(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public void CreateHeader()
        {
            InitialiseHeader(_loggingService);
            SetMessageDetails();
            SetSenderDetails();
        }

        public override void SetMessageDetails()
        {
            GovTalkMessageHeaderMessageDetails MessageDetails = new hmrcclasses.GovTalkMessageHeaderMessageDetails();
            MessageDetails.Class = ReferenceDataManager.Settings["MessageDetailsClass"];
            MessageDetails.Qualifier = GovTalkMessageHeaderMessageDetailsQualifier.request;
            MessageDetails.FunctionSpecified = true;
            MessageDetails.Function = GovTalkMessageHeaderMessageDetailsFunction.submit;
            MessageDetails.TransformationSpecified = true;
            MessageDetails.Transformation = GovTalkMessageHeaderMessageDetailsTransformation.XML;           

            // @TODO: Build SubmitRequestLocalTestService

            if (ReferenceDataManager.governmentGatewayEnvironment == GovernmentGatewayEnvironment.localtestservice)
            {
                MessageDetails.GatewayTimestampSpecified = true;
                MessageDetails.GatewayTimestamp = DateTime.Now;

            }
            else
            {
                MessageDetails.GatewayTimestampSpecified = false;
                MessageDetails.GatewayTimestamp = DateTime.MinValue;
            }

            if (ReferenceDataManager.governmentGatewayEnvironment == GovernmentGatewayEnvironment.localtestservice || ReferenceDataManager.governmentGatewayEnvironment == GovernmentGatewayEnvironment.devgateway)
            {
                MessageDetails.GatewayTest = ReferenceDataManager.Settings["MessageDetailsGatewayTest"];
            }

            Header.MessageDetails = MessageDetails;
        }
        public override void SetSenderDetails()
        {
            hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthentication Authentication = new hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthentication();
            switch (ReferenceDataManager.Settings["SenderAuthenticationMethod"])
            {
                case "MD5":
                    Authentication.Method = hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthenticationMethod.MD5;
                    break;
                case "clear":
                    Authentication.Method = hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthenticationMethod.clear;
                    break;
                case "W3Csigned":
                    Authentication.Method = hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthenticationMethod.W3Csigned;
                    break;
            }
            Authentication.Role = ReferenceDataManager.Settings["SenderAuthenticationRole"];
            Authentication.Item = ReferenceDataManager.Settings["SenderAuthenticationValue"];

            GovTalkMessageHeaderSenderDetailsIDAuthentication IDAuthentication = new hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthentication();
            IDAuthentication.SenderID = ReferenceDataManager.Settings["SenderID"];

            GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthentication[] Authentications = new hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthentication[1];
            Authentications[0] = Authentication;
            IDAuthentication.Authentication = Authentications;

            GovTalkMessageHeaderSenderDetails SenderDetails = new hmrcclasses.GovTalkMessageHeaderSenderDetails();
            SenderDetails.IDAuthentication = IDAuthentication;

            Header.SenderDetails = SenderDetails;
        }
    }

    public class PollHeaderBuilder : HeaderBuilderBase
    {
        private string _correlationId;
        private ILoggingService _loggingService;

        public PollHeaderBuilder(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
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
        public void CreateHeader()
        {
            InitialiseHeader(_loggingService);
            SetMessageDetails();
            SetSenderDetails();
        }

        public override void SetSenderDetails() {}
        public override void SetMessageDetails()
        {
            GovTalkMessageHeaderMessageDetails MessageDetails = new hmrcclasses.GovTalkMessageHeaderMessageDetails();
            MessageDetails.Class = ReferenceDataManager.Settings["MessageDetailsClass"];
            MessageDetails.Qualifier = hmrcclasses.GovTalkMessageHeaderMessageDetailsQualifier.poll;
            MessageDetails.FunctionSpecified = true;
            MessageDetails.Function = hmrcclasses.GovTalkMessageHeaderMessageDetailsFunction.submit;
            MessageDetails.TransformationSpecified = true;
            MessageDetails.Transformation = hmrcclasses.GovTalkMessageHeaderMessageDetailsTransformation.XML;
            
            // @TODO: This depends on whether it's a test ...
            MessageDetails.GatewayTest = ReferenceDataManager.Settings["MessageDetailsGatewayTest"];

            MessageDetails.CorrelationID = CorrelationId;
            MessageDetails.GatewayTimestampSpecified = false;
            MessageDetails.GatewayTimestamp = DateTime.MinValue;
            
            Header.MessageDetails = MessageDetails;
        }
    }

    public class DeleteHeaderBuilder : HeaderBuilderBase
    {
        private string _correlationId;
        private ILoggingService _loggingService;

        public DeleteHeaderBuilder(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
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
        public void CreateHeader()
        {
            InitialiseHeader(_loggingService);
            SetMessageDetails();
            SetSenderDetails();
        }

        public override void SetSenderDetails() { }

        public override void SetMessageDetails()
        {
            GovTalkMessageHeaderMessageDetails MessageDetails = new hmrcclasses.GovTalkMessageHeaderMessageDetails();
            MessageDetails.Class = ReferenceDataManager.Settings["MessageDetailsClass"];
            MessageDetails.Qualifier = hmrcclasses.GovTalkMessageHeaderMessageDetailsQualifier.request;
            MessageDetails.FunctionSpecified = true;
            MessageDetails.Function = hmrcclasses.GovTalkMessageHeaderMessageDetailsFunction.delete;
            MessageDetails.TransformationSpecified = true;
            MessageDetails.Transformation = hmrcclasses.GovTalkMessageHeaderMessageDetailsTransformation.XML;

            // @TODO: This depends on whether it's a test ...
            MessageDetails.GatewayTest = ReferenceDataManager.Settings["MessageDetailsGatewayTest"];

            MessageDetails.CorrelationID = CorrelationId;
            MessageDetails.GatewayTimestampSpecified = false;
            MessageDetails.GatewayTimestamp = DateTime.MinValue;

            Header.MessageDetails = MessageDetails;
        }
    }

    public class ListRequestHeaderBuilder : HeaderBuilderBase
    {
        private ILoggingService _loggingService;

        public ListRequestHeaderBuilder(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public void CreateHeader()
        {
            InitialiseHeader(_loggingService);
            SetMessageDetails();
            SetSenderDetails();
        }
        public override void SetMessageDetails()
        {
            GovTalkMessageHeaderMessageDetails MessageDetails = new hmrcclasses.GovTalkMessageHeaderMessageDetails();
            MessageDetails.Class = ReferenceDataManager.Settings["MessageDetailsClass"];
            MessageDetails.Qualifier = hmrcclasses.GovTalkMessageHeaderMessageDetailsQualifier.request;
            MessageDetails.FunctionSpecified = true;
            MessageDetails.Function = hmrcclasses.GovTalkMessageHeaderMessageDetailsFunction.list;
            MessageDetails.TransformationSpecified = true;
            MessageDetails.Transformation = hmrcclasses.GovTalkMessageHeaderMessageDetailsTransformation.XML;

            // @TODO: This depends on whether it's a test ...
            MessageDetails.GatewayTest = ReferenceDataManager.Settings["MessageDetailsGatewayTest"];

            MessageDetails.CorrelationID = String.Empty;
            MessageDetails.GatewayTimestampSpecified = false;
            MessageDetails.GatewayTimestamp = DateTime.MinValue;

            Header.MessageDetails = MessageDetails;
        }
        public override void SetSenderDetails()
        {
            GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthentication Authentication = new hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthentication();
            switch (ReferenceDataManager.Settings["SenderAuthenticationMethod"])
            {
                case "MD5":
                    Authentication.Method = hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthenticationMethod.MD5;
                    break;
                case "clear":
                    Authentication.Method = hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthenticationMethod.clear;
                    break;
                case "W3Csigned":
                    Authentication.Method = hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthenticationMethod.W3Csigned;
                    break;
            }
            Authentication.Role = ReferenceDataManager.Settings["SenderAuthenticationRole"];
            Authentication.Item = ReferenceDataManager.Settings["SenderAuthenticationValue"];

            GovTalkMessageHeaderSenderDetailsIDAuthentication IDAuthentication = new hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthentication();
            IDAuthentication.SenderID = ReferenceDataManager.Settings["SenderID"];

            GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthentication[] Authentications = new hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthentication[1];
            Authentications[0] = Authentication;
            IDAuthentication.Authentication = Authentications;

            GovTalkMessageHeaderSenderDetails SenderDetails = new hmrcclasses.GovTalkMessageHeaderSenderDetails();
            SenderDetails.IDAuthentication = IDAuthentication;

            Header.SenderDetails = SenderDetails;
        }
    }
}
