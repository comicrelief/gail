using System;
using System.Linq;

using System.Xml;
using System.Xml.Linq;

using hmrcclasses;

namespace CharitiesOnline.Strategies
{
    public class ReadAcknowledgementStrategy : IMessageReadStrategy
    {
        private GovTalkMessage _message;
        
        public bool IsMatch(XDocument inMessage)
        {
            XNamespace ns = "http://www.govtalk.gov.uk/CM/envelope";

            string qualifier = inMessage.Descendants(ns + "Qualifier").FirstOrDefault().Value;
            string function = inMessage.Descendants(ns + "Function").FirstOrDefault().Value;

            if (qualifier == "acknowledgement" && function == "submit")
            {
                _message = Helpers.DeserializeMessage(inMessage.ToXmlDocument());
                return true;
            }

            return false;
        }

        public T ReadMessage<T>(XDocument inMessage)
        {
            string correlationId = "";
            string[] acknowledgmentResults = new string[4];

            if(typeof(T) == typeof(string))
            {
                correlationId = _message.Header.MessageDetails.CorrelationID;

                return (T)Convert.ChangeType(correlationId, typeof(T));
            }

            if(typeof(T) == typeof(string[]))
            {
                acknowledgmentResults[0] = _message.Header.MessageDetails.CorrelationID;
                acknowledgmentResults[1] = _message.Header.MessageDetails.ResponseEndPoint.Value;
                acknowledgmentResults[2] = _message.Header.MessageDetails.ResponseEndPoint.PollInterval;
                acknowledgmentResults[3] = _message.Header.MessageDetails.GatewayTimestamp.ToString();

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
