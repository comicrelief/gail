using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using hmrcclasses;
using CharitiesOnline.Helpers;
using CR.Infrastructure.Logging;
using CR.Infrastructure.Configuration;

namespace CharitiesOnline.Strategies
{

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

        public T ReadMessage<T>()
        {
            var message = _readers.First(r => r.IsMatch(_inMessage));

            message.ReadMessage(_inMessage);                

            return message.GetMessageResults<T>();                              

        }

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

        public bool HasErrors()
        {
            return _readers.First(r => r.IsMatch(_inMessage)).HasErrors();
        }
    }
}
