using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using hmrcclasses;
using CharitiesOnline.Helpers;
using CR.Infrastructure.Logging;
using CR.Infrastructure.Configuration;

namespace CharitiesOnline.MessageReadingStrategies
{

    /// <summary>
    /// Reads messages that are passed into the constructor as XDocuments
    /// </summary>
    public class DefaultMessageReader : IMessageReader
    {
        private readonly List<IMessageReadStrategy> _readers;
        private ILoggingService _loggingService;
        private IConfigurationRepository _configurationRepository;
        
        private XDocument _inMessage;

        public DefaultMessageReader(ILoggingService loggingService, IConfigurationRepository configurationRepository, XDocument inMessage)
        {
            _loggingService = loggingService;
            _configurationRepository = configurationRepository;
            _inMessage = inMessage;

            _readers = new List<IMessageReadStrategy>();
            _readers.Add(new ReadSubmitRequestStrategy(_loggingService));
            _readers.Add(new ReadAcknowledgementStrategy(_loggingService));
            _readers.Add(new ReadResponseStrategy(_loggingService));
            _readers.Add(new ReadErrorStrategy(_loggingService, _configurationRepository));
            _readers.Add(new ReadPollStrategy(_loggingService));
            _readers.Add(new ReadListResponseStrategy(_loggingService));
        }

        /// <summary>
        /// Reads the message based on the IsMatch method of each strategy
        /// </summary>
        public void ReadMessage()
        {
            var message = _readers.First(r => r.IsMatch(_inMessage));
            message.ReadMessage(_inMessage);                                                                  
        }

        /// <summary>
        /// Get results, acceptable types usually include string, string[], datatable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetMessageResults<T>()
        {
            var message = _readers.First(r => r.IsMatch(_inMessage));
            return message.GetMessageResults<T>(); 
        }

        /// <summary>
        /// Return the underlying GovTalkMessage
        /// </summary>
        /// <returns></returns>
        public GovTalkMessage Message()
        {
            return _readers.First(r => r.IsMatch(_inMessage)).Message();
        }
        
        public T GetBody<T>()
        {
            return _readers.First(r => r.IsMatch(_inMessage)).GetBody<T>();
        }

        public string GetBodyType()
        {
            return _readers.First(r => r.IsMatch(_inMessage)).GetBodyType();
        }

        public string GetCorrelationId()
        {
            return _readers.First(r => r.IsMatch(_inMessage)).GetCorrelationId();
        }

        public string GetQualifier()
        {
            return _readers.First(r => r.IsMatch(_inMessage)).GetQualifier();
        }

        public string GetFunction()
        {
            return _readers.First(r => r.IsMatch(_inMessage)).GetFunction();
        }

        public bool HasErrors()
        {
            return _readers.First(r => r.IsMatch(_inMessage)).HasErrors();
        }
    }
}
