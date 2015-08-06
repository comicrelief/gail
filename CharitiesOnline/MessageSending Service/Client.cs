using System;
using System.Xml;
using System.Net;
using System.IO;

using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using CharitiesOnline.Helpers;
using CR.Infrastructure.Logging;

namespace CharitiesOnline.MessageService
{
    public class Client
    {
        private ILoggingService _loggingService;

        public Client(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public XmlDocument SendRequest(XmlDocument InputDocument, string SendURI)
        {
            byte[] xmlBytes = InputDocument.XmlToBytes();

            Request request = new Request(_loggingService);
            Response response = new Response(_loggingService);
            
            XmlDocument xmlResponse = response.ProcessResponse(
                response.GetResponse(
                    request.PrepareRequest(xmlBytes, SendURI)));
            

            return xmlResponse;
                        
        }
    }
}
