using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using hmrcclasses;

namespace CharitiesOnline.Strategies
{
    public interface IMessageReader
    {
        T ReadMessage<T>();
        GovTalkMessage Message();
        T GetBody<T>();
        string GetBodyType();
        string GetCorrelationId();
        bool HasErrors();
    }
}
