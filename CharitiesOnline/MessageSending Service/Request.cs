using System;
using System.Xml;
using System.Net;
using System.IO;

using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using CR.Infrastructure.Logging;

namespace CharitiesOnline.MessageService
{
    public class Request
    {
        private CookieContainer _cookieContainer = new CookieContainer();
        private ILoggingService _loggingService;

        public Request(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public HttpWebRequest PrepareRequest(byte[] bytes, string SendURI)
        {
            _loggingService.LogInfo(this, string.Concat("Send URI is ", SendURI));

            HttpWebRequest MyHTTPWebRequest = (HttpWebRequest)WebRequest.Create(SendURI);

            MyHTTPWebRequest.Method = "POST";           
            MyHTTPWebRequest.ContentType = "text/xml";
            MyHTTPWebRequest.CookieContainer = _cookieContainer;
            MyHTTPWebRequest.Timeout = System.Threading.Timeout.Infinite;

            //payload

            MyHTTPWebRequest.ContentLength = bytes.Length;

            Stream RequestStream = MyHTTPWebRequest.GetRequestStream();
            
            RequestStream.Write(bytes, 0, bytes.Length);            
            RequestStream.Close();

            _loggingService.LogInfo(this, "Request created.");

            return MyHTTPWebRequest;
        }
    }
}
