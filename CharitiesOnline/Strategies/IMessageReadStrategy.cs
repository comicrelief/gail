using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CharitiesOnline.Strategies
{
    interface IMessageReadStrategy
    {
        bool IsMatch(XDocument inMessage);

        // Need to constrain T ...
        T ReadMessage<T>(XDocument xd);

    }
}
