using System;
using System.Xml.Linq;
using System.Xml;
using System.Linq;

using hmrcclasses;
using CharitiesOnline.Helpers;
using CR.Infrastructure.Logging;

namespace CharitiesOnline.Strategies
{
    public class ReadPollStrategy : IMessageReadStrategy
    {
        private GovTalkMessage _message;
        private ILoggingService _loggingService;

        public ReadPollStrategy(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public bool IsMatch(XDocument inMessage)
        {
            XNamespace ns = "http://www.govtalk.gov.uk/CM/envelope";

            string qualifier = inMessage.Descendants(ns + "Qualifier").FirstOrDefault().Value;
            string function = inMessage.Descendants(ns + "Function").FirstOrDefault().Value;

            if(qualifier == "poll" && function == "submit")
            {
                _message = XmlSerializationHelpers.DeserializeMessage(inMessage.ToXmlDocument());

                _loggingService.LogInfo(this, "Message read. Response type is Poll.");

                return true;
            }

            return false;
        }

        public GovTalkMessage Message()
        {
            return _message;
        }

        public T GetBody<T>()
        {
            return default(T);
        }

        public T ReadMessage<T>(XDocument inMessage)
        {
            if(typeof(T) == typeof(string))
            {
                string correlationId = _message.Header.MessageDetails.CorrelationID;

                _loggingService.LogInfo(this, string.Concat("Poll CorrelationId is ", correlationId));

                return (T)Convert.ChangeType(correlationId, typeof(T));
            }
            
            return default(T);
        }

        public string GetBodyType()
        {
            // return Type of _body
            return String.Empty;
        }

    }
}
