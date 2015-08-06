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
    public class Response
    {
        private ILoggingService _loggingService;

        public Response(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public HttpWebResponse GetResponse(HttpWebRequest request)
        {
            _loggingService.LogInfo(this, "Trying to get response.");
            HttpWebResponse MyHTTPWebResponse = (HttpWebResponse)request.GetResponse();

            _loggingService.LogInfo(this, "Web response call finished.");

            if(MyHTTPWebResponse == null)
            {
                throw new Exception("Something unknown went wrong in getting the response");
            }

            return MyHTTPWebResponse;
        }

        public XmlDocument ProcessResponse(HttpWebResponse response)
        {
            if(response.StatusCode == HttpStatusCode.OK)
            {
                _loggingService.LogInfo(this, "Web response received.");
                using (Stream ResponseStream = response.GetResponseStream())
                using (System.Xml.XmlTextReader xmlReader = new XmlTextReader(ResponseStream))
                {
                    XmlDocument xmlReplyDoc = new XmlDocument();
                    xmlReplyDoc.Load(xmlReader);
                    return xmlReplyDoc;
                }            
            }            
            else
            {
                throw new Exception(String.Format("Something went at the server {0}. Status code {1}; Status message {2}", 
                    response.ResponseUri,
                    response.StatusCode, 
                    response.StatusDescription));
            }
            
        }
    }
}
