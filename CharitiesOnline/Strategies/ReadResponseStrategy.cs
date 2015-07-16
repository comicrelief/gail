﻿using System;
using System.Text;
using System.Linq;

using System.Data;
using System.Xml;
using System.Xml.Linq;

using hmrcclasses;

namespace CharitiesOnline.Strategies
{
    public class ReadResponseStrategy : IMessageReadStrategy
    {
        private GovTalkMessage _message;
        private SuccessResponse _body;
        public bool IsMatch(XDocument inMessage)
        {
            XNamespace ns = "http://www.govtalk.gov.uk/CM/envelope";

            string qualifier = inMessage.Descendants(ns + "Qualifier").FirstOrDefault().Value;
            string function = inMessage.Descendants(ns + "Function").FirstOrDefault().Value;

            if(qualifier == "response" && function == "submit")
            {
                _message = Helpers.DeserializeMessage(inMessage.ToXmlDocument());

                XmlDocument successXml = new XmlDocument();
                successXml.LoadXml(_message.Body.Any[0].OuterXml);

                _body = Helpers.DeserializeSuccessResponse(successXml);

                return true;
            }

            return false;
        }

        public T ReadMessage<T>(XDocument inMessage)
        {           
            if(typeof(T) == typeof(string))
            {
                string correlationId = _message.Header.MessageDetails.CorrelationID;

                return (T)Convert.ChangeType(correlationId, typeof(T));
            }
            if(typeof(T) == typeof(string[]))
            {
                //correlationId, responseEndPoint, gatewayTimestamp, IRmarkReceipt.Message, AcceptedTime
                string[] response = new string[5];
                response[0] = _message.Header.MessageDetails.CorrelationID;
                response[1] = _message.Header.MessageDetails.ResponseEndPoint.Value;
                response[2] = _message.Header.MessageDetails.GatewayTimestamp.ToString();
                response[3] = _body.IRmarkReceipt.Message.Value;
                response[4] = _body.AcceptedTime.ToString();

                return (T)Convert.ChangeType(response, typeof(T));
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
    }
}