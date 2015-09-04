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
    public class ReadListResponseStrategy : IMessageReadStrategy
    {
        private GovTalkMessage _message;
        private GovTalkMessageBodyStatusReport _statusReport;
        private ILoggingService _loggingService;
        private string _qualifier;
        private string _function;
        private bool _messageRead;

        public ReadListResponseStrategy(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public bool IsMatch(XDocument inMessage)
        {
            XNamespace ns = "http://www.govtalk.gov.uk/CM/envelope";

            string qualifier = inMessage.Descendants(ns + "Qualifier").FirstOrDefault().Value;
            string function = inMessage.Descendants(ns + "Function").FirstOrDefault().Value;

            if(qualifier == "response" && function == "list" )
            {
                return true;
            }

            return false;
        }

        public void ReadMessage(XDocument inMessage)
        {
            _message = XmlSerializationHelpers.DeserializeMessage(inMessage.ToXmlDocument());

            _messageRead = true;

            _qualifier = _message.Header.MessageDetails.Qualifier.ToString();
            _function = _message.Header.MessageDetails.Function.ToString();

            XmlDocument listXml = new XmlDocument();

            listXml.LoadXml(_message.Body.Any[0].OuterXml);

            _statusReport = XmlSerializationHelpers.DeserializeStatusReport(listXml);

            _loggingService.LogInfo(this, "Message read. Response type is List Response.");

        }

        public T GetMessageResults<T>()
        {
            if (!_messageRead)
                throw new Exception("Message not read. Call ReadMessage first.");

            if(typeof(T) == typeof(string[]))
            {
                string[] listResults = new string[5];

                listResults[0] = string.Concat("CorrelationID::", _message.Header.MessageDetails.CorrelationID);
                listResults[1] = string.Concat("Qualifier::", _message.Header.MessageDetails.Qualifier.ToString());
                listResults[2] = string.Concat("ResponseEndPoint::", _message.Header.MessageDetails.ResponseEndPoint.Value);
                listResults[3] = string.Concat("PollInterval::", _message.Header.MessageDetails.ResponseEndPoint.PollInterval);
                listResults[4] = string.Concat("GatewayTimestamp::", _message.Header.MessageDetails.GatewayTimestamp.ToString());

                return (T)Convert.ChangeType(listResults, typeof(T));
            }

            if(typeof(T) == typeof(DataTable))
            {
                return (T)Convert.ChangeType(
                    DataHelpers.MakeStatusReportTable(_statusReport),
                        typeof(T));
            }

            return default(T);
        }

        public GovTalkMessage Message()
        {
            return _message;
        }

        public T GetBody<T>()
        {
            if (typeof(T) == typeof(GovTalkMessageBodyStatusReport))
            {
                return (T)Convert.ChangeType(_statusReport, typeof(T));
            }

            return default(T);
        }

        public string GetBodyType()
        {
            return _statusReport.GetType().ToString();
        }

        public string GetCorrelationId()
        {
            return string.Empty;
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

    }
}
