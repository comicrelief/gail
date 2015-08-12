using System;
using System.Linq;

using System.Xml;
using System.Xml.Linq;

using hmrcclasses;
using CharitiesOnline.Helpers;
using CR.Infrastructure.Logging;

namespace CharitiesOnline.Strategies
{
    public class ReadAcknowledgementStrategy : IMessageReadStrategy
    {
        private GovTalkMessage _message;
        private ILoggingService _loggingService;

        public ReadAcknowledgementStrategy(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        
        public bool IsMatch(XDocument inMessage)
        {
            XNamespace ns = "http://www.govtalk.gov.uk/CM/envelope";

            string qualifier = inMessage.Descendants(ns + "Qualifier").FirstOrDefault().Value;
            string function = inMessage.Descendants(ns + "Function").FirstOrDefault().Value;

            if (qualifier == "acknowledgement" && function == "submit")
            {
                _message = XmlSerializationHelpers.DeserializeMessage(inMessage.ToXmlDocument());

                _loggingService.LogInfo(this, "Message read. Response type is Acknowledgment.");

                return true;
            }

            return false;
        }

        public T ReadMessage<T>(XDocument inMessage)
        {
            string correlationId = "";
            string[] acknowledgmentResults = new string[5];

            if(typeof(T) == typeof(string))
            {
                correlationId = _message.Header.MessageDetails.CorrelationID;
                
                _loggingService.LogInfo(this, string.Concat("Acknowledgment CorrelationId is ",correlationId));

                return (T)Convert.ChangeType(correlationId, typeof(T));
            }

            if(typeof(T) == typeof(string[]))
            {
                acknowledgmentResults[0] = string.Concat("CorrelationID::",_message.Header.MessageDetails.CorrelationID);
                acknowledgmentResults[1] = string.Concat("Qualifier::",_message.Header.MessageDetails.Qualifier.ToString());
                acknowledgmentResults[2] = string.Concat("ResponseEndPoint::",_message.Header.MessageDetails.ResponseEndPoint.Value);
                acknowledgmentResults[3] = string.Concat("PollInterval::", _message.Header.MessageDetails.ResponseEndPoint.PollInterval);
                acknowledgmentResults[4] = string.Concat("GatewayTimestamp::",_message.Header.MessageDetails.GatewayTimestamp.ToString());

                _loggingService.LogInfo(this, string.Concat("Acknowledgment CorrelationId is ", acknowledgmentResults[0]));

                return (T)Convert.ChangeType(acknowledgmentResults, typeof(T));
            }

            return default(T);
        }

        public GovTalkMessage Message()
        {
            return _message;
        }

        public T GetBody<T>()
        {
            return default(T);
        }

        public string GetBodyType()
        {
            // return Type of _body
            return String.Empty;
        }
    }
}
