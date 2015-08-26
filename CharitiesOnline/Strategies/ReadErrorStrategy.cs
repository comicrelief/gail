using System;
using System.Xml.Linq;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

using hmrcclasses;
using CharitiesOnline.Helpers;
using CharitiesOnline.Models;

using CR.Infrastructure.Logging;
using CR.Infrastructure.Configuration;

namespace CharitiesOnline.Strategies
{
    public class ReadErrorStrategy : IMessageReadStrategy
    {
        private GovTalkMessage _message;
        private ErrorResponse _body;
        private List<GovTalkMessageGovTalkDetailsError> _govTalkDetailsErrors; // not sure ...        
        private ILoggingService _loggingService;
        private IConfigurationRepository _configurationRepository;
        private string _errorText;
        private string _correlationId = "";
        private string _qualifier;
        private string _function;
        private bool _messageRead;

        public ReadErrorStrategy(ILoggingService loggingService, IConfigurationRepository configurationRepository)
        {
            _loggingService = loggingService;
            _configurationRepository = configurationRepository;
        }
        
        public bool IsMatch(XDocument inMessage)
        {
            XNamespace ns = "http://www.govtalk.gov.uk/CM/envelope";

            string qualifier = inMessage.Descendants(ns + "Qualifier").FirstOrDefault().Value;
            string function = inMessage.Descendants(ns + "Function").FirstOrDefault().Value;

            if (qualifier == "error" && function == "submit")
            {               
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

        public void ReadMessage(XDocument inMessage)
        {
            try
            {
                _message = XmlSerializationHelpers.DeserializeMessage(inMessage.ToXmlDocument());

                _messageRead = true;

                _correlationId = _message.Header.MessageDetails.CorrelationID;
                _qualifier = _message.Header.MessageDetails.Qualifier.ToString();
                _function = _message.Header.MessageDetails.Function.ToString();

                if (_message.Body.Any != null)
                {
                    XmlDocument errorXml = new XmlDocument();
                    errorXml.LoadXml(_message.Body.Any[0].OuterXml);
                    _body = XmlSerializationHelpers.DeserializeErrorResponse(errorXml);
                    _errorText = _body.Application.Any[0].Name + ":" + _body.Application.Any[0].InnerText;
                }

                if (_message.GovTalkDetails != null)
                {
                    if (_message.GovTalkDetails.GovTalkErrors != null)
                    {
                        _govTalkDetailsErrors = new List<GovTalkMessageGovTalkDetailsError>();

                        foreach (GovTalkMessageGovTalkDetailsError error in _message.GovTalkDetails.GovTalkErrors)
                        {
                            _govTalkDetailsErrors.Add(error);
                        }

                        _errorText = String.Format("There are {0} GovTalkDetailsErrors.", _govTalkDetailsErrors.Count);
                    }
                }

                _loggingService.LogInfo(this, string.Concat("Message read. Response type is Error."));
            }
            catch(Exception ex)
            {
                _loggingService.LogError(this, "Message Reading Exception", ex);

                GovTalkMessageFileName FileNamer = new GovTalkMessageFileName(_loggingService,_configurationRepository);
                string filename = FileNamer.DefaultFileName();

                _loggingService.LogInfo(this, String.Concat("Attempting to save reply document to ", filename, "."));

                inMessage.Save(filename);
            }            
        }

        public T GetMessageResults<T>()
        {
            if (!_messageRead)
                throw new Exception("Message not read. Call ReadMessage first.");

            if (typeof(T) == typeof(string))
            {
                string correlationId = _message.Header.MessageDetails.CorrelationID;

                _loggingService.LogInfo(this, string.Concat("Error CorrelationId is ", correlationId));

                return (T)Convert.ChangeType(correlationId, typeof(T));
            }
            if (typeof(T) == typeof(string[]))
            {
                string[] response = new string[5];
                response[0] = string.Concat("CorrelationId::", _message.Header.MessageDetails.CorrelationID);
                response[1] = string.Concat("Qualifier::", _message.Header.MessageDetails.Qualifier);
                response[2] = string.Concat("ResponseEndPoint::", _message.Header.MessageDetails.ResponseEndPoint.Value);
                response[3] = string.Concat("GatewayTimestamp::", _message.Header.MessageDetails.GatewayTimestamp.ToString());
                response[4] = string.Concat("Error::", _errorText);

                _loggingService.LogInfo(this, string.Concat("Error CorrelationId is ", response[0]));

                return (T)Convert.ChangeType(response, typeof(T));
            }
            if(typeof(T) == typeof(GatewayError))
            {
                foreach(var govTalkError in _message.GovTalkDetails.GovTalkErrors)
                {
                    GatewayError error = new GatewayError
                    {
                        ErrorCode = Convert.ToInt32(govTalkError.Number),
                        ErrorDescription = govTalkError.Text[0]
                    };

                    // @TODO: What if there are more than one? The schema allows this, 
                    // If the user has asked for a single GatewayError object, how to cope?
                    // Make GatewayError object a class that holds a list of errors ( i.e. it's GovTalkErrors not GovTalkDetailsGovTalkError)
                    // or, return first error if a single object is T, multiple errors if T is a collection?

                    return (T)Convert.ChangeType(error, typeof(T));
                }
            }

            return default(T);
        }

        public string GetBodyType()
        {
            // return Type of _body
            return _body.GetType().ToString();
        }

        public string GetCorrelationId()
        {
            return _correlationId;
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
            return true;
        }

    }
}
