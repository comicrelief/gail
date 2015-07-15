using System;
using System.Xml.Linq;
using System.Xml;
using System.Linq;

using hmrcclasses;

namespace CharitiesOnline.Strategies
{
    public class ReadErrorStrategy : IMessageReadStrategy
    {
        private GovTalkMessage _message;
        private ErrorResponse _body;
        public bool IsMatch(XDocument inMessage)
        {
            XNamespace ns = "http://www.govtalk.gov.uk/CM/envelope";

            string qualifier = inMessage.Descendants(ns + "Qualifier").FirstOrDefault().Value;
            string function = inMessage.Descendants(ns + "Function").FirstOrDefault().Value;

            if (qualifier == "error" && function == "submit")
            {
                _message = Helpers.DeserializeMessage(inMessage.ToXmlDocument());
                
                XmlDocument errorXml = new XmlDocument();
                errorXml.LoadXml(_message.Body.Any[0].OuterXml);

                _body = Helpers.DeserializeErrorResponse(errorXml);

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
            return (T)Convert.ChangeType(_body, typeof(T));
        }

        public T ReadMessage<T>(XDocument inMessage)
        {
            if (typeof(T) == typeof(string))
            {
                string correlationId = _message.Header.MessageDetails.CorrelationID;

                return (T)Convert.ChangeType(correlationId, typeof(T));
            }
            if (typeof(T) == typeof(string[]))
            {
                //correlationId, responseEndPoint, gatewayTimestamp, IRmarkReceipt.Message, AcceptedTime
                string[] response = new string[5];
                response[0] = _message.Header.MessageDetails.CorrelationID;
                response[1] = _message.Header.MessageDetails.ResponseEndPoint.Value;
                response[2] = _message.Header.MessageDetails.GatewayTimestamp.ToString();
                response[3] = _body.Application.Any[0].Name + ":" + _body.Application.Any[0].InnerText;
            }

            return default(T);
        }

    }
}
