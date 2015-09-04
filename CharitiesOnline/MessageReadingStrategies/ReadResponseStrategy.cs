using System;
using System.Text;
using System.Linq;

using System.Data;
using System.Xml;
using System.Xml.Linq;

using hmrcclasses;
using CharitiesOnline.Helpers;
using CR.Infrastructure.Logging;

namespace CharitiesOnline.MessageReadingStrategies
{
    public class ReadResponseStrategy : IMessageReadStrategy
    {
        private GovTalkMessage _message;
        private SuccessResponse _body;
        private ILoggingService _loggingService;
        private string _correlationId;
        private string _qualifier;
        private string _function;
        private bool _messageRead;

        public ReadResponseStrategy(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public bool IsMatch(XDocument inMessage)
        {
            XNamespace ns = "http://www.govtalk.gov.uk/CM/envelope";

            string qualifier = inMessage.Descendants(ns + "Qualifier").FirstOrDefault().Value;
            string function = inMessage.Descendants(ns + "Function").FirstOrDefault().Value;

            if(qualifier == "response" && function == "submit")
            {
                return true;
            }

            return false;
        }

        public void ReadMessage(XDocument inMessage)
        {
            _message = XmlSerializationHelpers.DeserializeMessage(inMessage.ToXmlDocument());
            _messageRead = true;
            _correlationId = _message.Header.MessageDetails.CorrelationID;
            _qualifier = _message.Header.MessageDetails.Qualifier.ToString();
            _function = _message.Header.MessageDetails.Function.ToString();

            XmlDocument successXml = new XmlDocument();
            
            successXml.LoadXml(_message.Body.Any[0].OuterXml);

            _body = XmlSerializationHelpers.DeserializeSuccessResponse(successXml);

            _loggingService.LogInfo(this, "Message read. Response type is Response.");
        }

        public T GetMessageResults<T>()
        {
            if (!_messageRead)
                throw new Exception("Message not read. Call ReadMessage first.");

            if (typeof(T) == typeof(string))
            {
                string correlationId = _message.Header.MessageDetails.CorrelationID;

                _loggingService.LogInfo(this, string.Concat("Response CorrelationId is ", correlationId));

                return (T)Convert.ChangeType(correlationId, typeof(T));
            }
            if (typeof(T) == typeof(string[]))
            {
                //correlationId, responseEndPoint, gatewayTimestamp, IRmarkReceipt.Message, AcceptedTime
                string[] response = new string[6];
                response[0] = string.Concat("CorrelationId::", _message.Header.MessageDetails.CorrelationID);
                response[1] = string.Concat("Qualifier::", _message.Header.MessageDetails.Qualifier);
                response[2] = string.Concat("ResponseEndPoint::", _message.Header.MessageDetails.ResponseEndPoint.Value);
                response[3] = string.Concat("GatewayTimestamp::", _message.Header.MessageDetails.GatewayTimestamp.ToString());
                response[4] = string.Concat("IRmarkReceipt::", _body.IRmarkReceipt.Message.Value);
                response[5] = string.Concat("AcceptedTime::", _body.AcceptedTime.ToString());

                _loggingService.LogInfo(this, string.Concat("Response CorrelationId is ", response[0]));

                return (T)Convert.ChangeType(response, typeof(T));
            }
            if(typeof(T) == typeof(DataTable))
            {
                return (T)Convert.ChangeType(GetDataTableResponse(), typeof(T));
            }

            return default(T);
        }

        public GovTalkMessage Message()
        {
            return _message;
        }

        public T GetBody<T>()
        {
            if(typeof(T) == typeof(SuccessResponse))
            {
                return (T)Convert.ChangeType(_body, typeof(T));
            }

            return default(T);
        }
        public string GetBodyType()
        {
            // return Type of _body
            return _body.GetType().ToString();
        }
        public string GetCorrelationId()
        {
            return _correlationId;
        }
        public string GetQualifier()
        {
            return _qualifier;
        }
        public string GetFunction()
        {
            return _function;
        }           
        public bool HasErrors()
        {
            return false;
        }

        private DataTable GetDataTableResponse()
        {
            DataTable responseTable = DataHelpers.MakeResponseTable(_body, _correlationId);

            return responseTable;            
        }
        
    }
}
