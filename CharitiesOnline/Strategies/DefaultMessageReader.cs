using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using hmrcclasses;

namespace CharitiesOnline.Strategies
{

    public class DefaultMessageReader : IMessageReader
    {
        private readonly List<IMessageReadStrategy> _readers;

        public DefaultMessageReader()
        {
            _readers = new List<IMessageReadStrategy>();
            _readers.Add(new ReadSubmitRequestStrategy());
            _readers.Add(new ReadAcknowledgementStrategy());
            _readers.Add(new ReadResponseStrategy());
        }

        public T ReadMessage<T>(XDocument inMessage)
        {
            return _readers.First(r => r.IsMatch(inMessage)).ReadMessage<T>(inMessage);
        }

        public GovTalkMessage Message(XDocument inMessage)
        {
            return _readers.First(r => r.IsMatch(inMessage)).Message();
        }

        public T GetBody<T>()
        {
            return default(T);
        }
    }
}
