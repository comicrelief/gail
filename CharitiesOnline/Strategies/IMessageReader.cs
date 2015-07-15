using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using hmrcclasses;

namespace CharitiesOnline.Strategies
{
    public interface IMessageReader
    {
        T ReadMessage<T>(XDocument inMessage);
        GovTalkMessage Message(XDocument inMessage);
        T GetBody<T>(XDocument inMessage);
    }
}
