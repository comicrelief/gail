using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using hmrcclasses;
using CharitiesOnline.Helpers;
using CR.Infrastructure.Logging;

namespace CharitiesOnline.Strategies
{

    public class DefaultMessageReader : IMessageReader
    {
        private readonly List<IMessageReadStrategy> _readers;
        private ILoggingService _loggingService;

        public DefaultMessageReader(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _readers = new List<IMessageReadStrategy>();
            _readers.Add(new ReadSubmitRequestStrategy());
            _readers.Add(new ReadAcknowledgementStrategy(_loggingService));
            _readers.Add(new ReadResponseStrategy(_loggingService));
            _readers.Add(new ReadErrorStrategy(_loggingService));
            _readers.Add(new ReadPollStrategy(_loggingService));
        }

        public T ReadMessage<T>(XDocument inMessage)
        {
            return _readers.First(r => r.IsMatch(inMessage)).ReadMessage<T>(inMessage);
        }

        public GovTalkMessage Message(XDocument inMessage)
        {
            return _readers.First(r => r.IsMatch(inMessage)).Message();
        }

        public T GetBody<T>(XDocument inMessage)
        {
            return _readers.First(r => r.IsMatch(inMessage)).GetBody<T>();
        }

        public string GetBodyType(XDocument inMessage)
        {
            return _readers.First(r => r.IsMatch(inMessage)).GetBodyType();
        }
    }
}
