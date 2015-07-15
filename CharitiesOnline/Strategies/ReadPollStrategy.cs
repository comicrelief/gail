using System;
using System.Xml.Linq;
using System.Xml;
using System.Linq;

using hmrcclasses;

namespace CharitiesOnline.Strategies
{
    public class ReadPollStrategy : IMessageReadStrategy
    {
        private GovTalkMessage _message;
        public bool IsMatch(XDocument inMessage)
        {
            XNamespace ns = "http://www.govtalk.gov.uk/CM/envelope";

            string qualifier = inMessage.Descendants(ns + "Qualifier").FirstOrDefault().Value;
            string function = inMessage.Descendants(ns + "Function").FirstOrDefault().Value;

            if(qualifier == "poll" && function == "submit")
            {
                _message = Helpers.DeserializeMessage(inMessage.ToXmlDocument());

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

                return (T)Convert.ChangeType(correlationId, typeof(T));
            }
            
            return default(T);
        }

    }
}
