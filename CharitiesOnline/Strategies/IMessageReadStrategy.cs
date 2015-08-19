using System;
using System.Collections.Generic;
using System.Xml.Linq;

using hmrcclasses;
using CharitiesOnline.Helpers;

namespace CharitiesOnline.Strategies
{
    interface IMessageReadStrategy
    {
        bool IsMatch(XDocument inMessage);

        // Need to constrain T ...
        void ReadMessage(XDocument xd);

        T GetMessageResults<T>();

        GovTalkMessage Message();

        T GetBody<T>();

        string GetBodyType();

        string GetCorrelationId();

        bool HasErrors();

    }
}
