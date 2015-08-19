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
        private string _correlationId;

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

        public void ReadMessage(XDocument inMessage)
        {
            _message = XmlSerializationHelpers.DeserializeMessage(inMessage.ToXmlDocument());
            
            _correlationId = _message.Header.MessageDetails.CorrelationID;

            
        }

        public T GetMessageResults<T>()
        {
            if (typeof(T) == typeof(string))
            {               
                _loggingService.LogInfo(this, string.Concat("Poll CorrelationId is ", _correlationId));

                return (T)Convert.ChangeType(_correlationId, typeof(T));
            }

            return default(T);
        }

        public string GetBodyType()
        {
            // return Type of _body
            return String.Empty;
        }

        public string GetCorrelationId()
        {
            return _correlationId;
        }

        public bool HasErrors()
        {
            return false;
        }


    }
}
