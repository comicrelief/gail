using System;
using System.Collections.Generic;
using System.Xml.Linq;

using hmrcclasses;
using CharitiesOnline.Helpers;

namespace CharitiesOnline.MessageReadingStrategies
{
    interface IMessageReadStrategy
    {
        bool IsMatch(XDocument inMessage);

        void ReadMessage(XDocument xd);

        // Need to constrain T ...
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
