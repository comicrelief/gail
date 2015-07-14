using System;
using System.Text;
using System.Linq;

using System.Data;
using System.Xml;
using System.Xml.Linq;

using hmrcclasses;

namespace CharitiesOnline.Strategies
{
    public class ReadResponse :IMessageReadStrategy
    {
        private GovTalkMessage _message;
        private SuccessResponse _body;
        public bool IsMatch(XDocument inMessage)
        {
            _message = Helpers.DeserializeMessage(inMessage.ToXmlDocument());

            XmlDocument successXml = new XmlDocument();
            successXml.LoadXml(_message.Body.Any[0].OuterXml);

            _body = Helpers.DeserializeSuccessResponse(successXml);

            if(_message.Header.MessageDetails.Qualifier == GovTalkMessageHeaderMessageDetailsQualifier.response && _message.Header.MessageDetails.Function == GovTalkMessageHeaderMessageDetailsFunction.submit)
            {
                return true;
            }

            return false;
        }

        public T ReadMessage<T>(XDocument inMessage)
        {           
            if(typeof(T) == typeof(string))
            {
                
            }

            return default(T);
        }

        public GovTalkMessage Message()
        {
            return _message;
        }

        public T GetBody<T>()
        {
            return (T)Convert.ChangeType(_body, typeof(T));
        }
    }
}
