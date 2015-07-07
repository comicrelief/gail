using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharitiesOnlineWorkings.Builders
{
    public abstract class HeaderBuilderBase
    {
        private hmrcclasses.GovTalkMessageHeader _header;

        public hmrcclasses.GovTalkMessageHeader Header
        {
            get
            {
                return _header;
            }
        }

        public void InitialiseHeader()
        {
            _header = new hmrcclasses.GovTalkMessageHeader();
        }

        public abstract void SetMessageDetails();
        public abstract void SetSenderDetails();
    }

    public class HeaderCreator
    {
        private HeaderBuilderBase _headerBuilder;

        public HeaderCreator(HeaderBuilderBase headerBuilder)
        {
            _headerBuilder = headerBuilder;
        }

        public void CreateHeader()
        {
            _headerBuilder.InitialiseHeader();
            _headerBuilder.SetMessageDetails();
            _headerBuilder.SetSenderDetails();
        }

        public hmrcclasses.GovTalkMessageHeader GetHeader()
        {
            return _headerBuilder.Header;
        }
    }

    public class RequestHeaderBuilder : HeaderBuilderBase
    {
        public void CreateHeader()
        {
            InitialiseHeader();
            SetMessageDetails();
            SetSenderDetails();
        }

        public override void SetMessageDetails()
        {
            hmrcclasses.GovTalkMessageHeaderMessageDetails MessageDetails = new hmrcclasses.GovTalkMessageHeaderMessageDetails();
            MessageDetails.Class = ReferenceDataManager.Settings["MessageDetailsClass"];
            MessageDetails.Qualifier = hmrcclasses.GovTalkMessageHeaderMessageDetailsQualifier.request;
            MessageDetails.FunctionSpecified = true;
            MessageDetails.Function = hmrcclasses.GovTalkMessageHeaderMessageDetailsFunction.submit;
            MessageDetails.TransformationSpecified = true;
            MessageDetails.Transformation = hmrcclasses.GovTalkMessageHeaderMessageDetailsTransformation.XML;

            // @TODO: Does GatewayTest need to be omitted for live submit requests?

            MessageDetails.GatewayTest = ReferenceDataManager.Settings["MessageDetailsGatewayTest"];

            // @TODO: Build SubmitRequestLocalTestService

            if (ReferenceDataManager.Settings["env"] == "local")
            {
                MessageDetails.GatewayTimestampSpecified = true;
                MessageDetails.GatewayTimestamp = DateTime.Now;
            }
            else
            {
                MessageDetails.GatewayTimestampSpecified = false;
                MessageDetails.GatewayTimestamp = DateTime.MinValue;
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

            hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthentication IDAuthentication = new hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthentication();
            IDAuthentication.SenderID = ReferenceDataManager.Settings["SenderID"];

            hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthentication[] Authentications = new hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthentication[1];
            Authentications[0] = Authentication;
            IDAuthentication.Authentication = Authentications;

            hmrcclasses.GovTalkMessageHeaderSenderDetails SenderDetails = new hmrcclasses.GovTalkMessageHeaderSenderDetails();
            SenderDetails.IDAuthentication = IDAuthentication;

            Header.SenderDetails = SenderDetails;
        }
    }

    public class PollHeaderBuilder : HeaderBuilderBase
    {
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
        public void CreateHeader()
        {
            InitialiseHeader();
            SetMessageDetails();
            SetSenderDetails();
        }

        public override void SetSenderDetails() {}
        public override void SetMessageDetails()
        {
            hmrcclasses.GovTalkMessageHeaderMessageDetails MessageDetails = new hmrcclasses.GovTalkMessageHeaderMessageDetails();
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
            InitialiseHeader();
            SetMessageDetails();
            SetSenderDetails();
        }

        public override void SetSenderDetails() { }

        public override void SetMessageDetails()
        {
            hmrcclasses.GovTalkMessageHeaderMessageDetails MessageDetails = new hmrcclasses.GovTalkMessageHeaderMessageDetails();
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
        public void CreateHeader()
        {
            InitialiseHeader();
            SetMessageDetails();
            SetSenderDetails();
        }
        public override void SetMessageDetails()
        {
            hmrcclasses.GovTalkMessageHeaderMessageDetails MessageDetails = new hmrcclasses.GovTalkMessageHeaderMessageDetails();
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

            hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthentication IDAuthentication = new hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthentication();
            IDAuthentication.SenderID = ReferenceDataManager.Settings["SenderID"];

            hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthentication[] Authentications = new hmrcclasses.GovTalkMessageHeaderSenderDetailsIDAuthenticationAuthentication[1];
            Authentications[0] = Authentication;
            IDAuthentication.Authentication = Authentications;

            hmrcclasses.GovTalkMessageHeaderSenderDetails SenderDetails = new hmrcclasses.GovTalkMessageHeaderSenderDetails();
            SenderDetails.IDAuthentication = IDAuthentication;

            Header.SenderDetails = SenderDetails;
        }
    }
}
