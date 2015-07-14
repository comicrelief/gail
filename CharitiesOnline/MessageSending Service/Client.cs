using System;
using System.Xml;
using System.Net;
using System.IO;

using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CharitiesOnline.MessageService
{
    public class Client
    {
        public XmlDocument SendRequest(XmlDocument InputDocument, string SendURI)
        {            
            byte[] xmlBytes = Helpers.XmlToBytes(InputDocument);

            Request request = new Request();
            Response response = new Response();
            
            XmlDocument xmlResponse = response.ProcessResponse(
                response.GetResponse(
                    request.PrepareRequest(xmlBytes, SendURI)));

            return xmlResponse;
                        
        }
    }
}
