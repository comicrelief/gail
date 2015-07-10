using System;
using System.Xml;
using System.Net;
using System.IO;

using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CharitiesOnlineWorkings.MessageService
{
    public class Request
    {
        private CookieContainer _cookieContainer = new CookieContainer();
        public HttpWebRequest PrepareRequest(byte[] bytes, string SendURI)
        {
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

            return MyHTTPWebRequest;
        }
    }
}
