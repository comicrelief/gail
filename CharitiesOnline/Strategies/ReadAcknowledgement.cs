using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Linq;

using hmrcclasses;

namespace CharitiesOnline.Strategies
{
    public class ReadAcknowledgement : IMessageReadStrategy
    {
        private GovTalkMessage _message;
        

        public bool IsMatch(XDocument inMessage)
        {
            _message = Helpers.DeserializeMessage(inMessage.ToXmlDocument());

            if (_message.Header.MessageDetails.Qualifier == GovTalkMessageHeaderMessageDetailsQualifier.acknowledgement && _message.Header.MessageDetails.Function == GovTalkMessageHeaderMessageDetailsFunction.submit)
            {
                return true;
            }

            return false;
        }

        public T ReadMessage<T>(XDocument inMessage)
        {
            string correlationId = "";
            string[] acknowledgmentResults = new string[4];

            GovTalkMessage acknowldgement = Helpers.DeserializeMessage(inMessage.ToXmlDocument());

            if(typeof(T) == typeof(string))
            {
                correlationId = acknowldgement.Header.MessageDetails.CorrelationID;

                return (T)Convert.ChangeType(correlationId, typeof(T));
            }

            if(typeof(T) == typeof(string[]))
            {
                acknowledgmentResults[0] = acknowldgement.Header.MessageDetails.CorrelationID;
                acknowledgmentResults[1] = acknowldgement.Header.MessageDetails.ResponseEndPoint.Value;
                acknowledgmentResults[2] = acknowldgement.Header.MessageDetails.ResponseEndPoint.PollInterval;
                acknowledgmentResults[3] = acknowldgement.Header.MessageDetails.GatewayTimestamp.ToString();

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
            //return (T)Convert.ChangeType(_body, typeof(T));
        }
    }
}
