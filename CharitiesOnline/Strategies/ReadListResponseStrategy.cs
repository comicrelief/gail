using System;
using System.Text;
using System.Linq;

using System.Data;
using System.Xml;
using System.Xml.Linq;

using hmrcclasses;
using CharitiesOnline.Helpers;
using CR.Infrastructure.Logging;

namespace CharitiesOnline.Strategies
{
    public class ReadListResponseStrategy : IMessageReadStrategy
    {
        private GovTalkMessage _message;
        private GovTalkMessageBodyStatusReport _statusReport;
        private ILoggingService _loggingService;

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

            XmlDocument listXml = new XmlDocument();

            listXml.LoadXml(_message.Body.Any[0].OuterXml);

            _statusReport = XmlSerializationHelpers.DeserializeStatusReport(listXml);

            _loggingService.LogInfo(this, "Message read. Response type is List Response.");

        }

        public T GetMessageResults<T>()
        {
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

        public bool HasErrors()
        {
            return false;
        }

    }
}
