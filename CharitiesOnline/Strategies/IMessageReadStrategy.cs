using System;
using System.Collections.Generic;
using System.Xml.Linq;

using hmrcclasses;

namespace CharitiesOnline.Strategies
{
    interface IMessageReadStrategy
    {
        bool IsMatch(XDocument inMessage);

        // Need to constrain T ...
        T ReadMessage<T>(XDocument xd);

        GovTalkMessage Message();

        T GetBody<T>();

    }
}
