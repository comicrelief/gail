using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Linq;

namespace CharitiesOnline.Strategies
{
    public class ReadAcknowledgement : IMessageReadStrategy
    {
        public bool IsMatch(XDocument inMessage)
        {
            return false;
        }

        public T ReadMessage<T>(XDocument inMessage)
        {
            string correlationId = ""; 

            if(typeof(T) == typeof(string))
            {
                // get correlation Id
            }

            return (T)Convert.ChangeType(correlationId, typeof(T));
        }
    }
}
