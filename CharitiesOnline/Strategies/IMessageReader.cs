using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using hmrcclasses;

namespace CharitiesOnline.Strategies
{
    public interface IMessageReader
    {
        void ReadMessage();
        T GetMessageResults<T>();
        GovTalkMessage Message();
        T GetBody<T>();
        string GetBodyType();
        string GetCorrelationId();
        string GetQualifier();
        string GetFunction();
        bool HasErrors();
    }
}
