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
                _loggingService.LogDebug(this, "Content-Type is " + response.ContentType);

                XmlDocument xmlReplyDoc = new XmlDocument();
                using (Stream ResponseStream = response.GetResponseStream())
                {
                    if(response.ContentType != "") //response Content-Type from LTS appears to be text/html
                    {
                        using (System.Xml.XmlTextReader xmlReader = new XmlTextReader(ResponseStream))
                        {
                            xmlReplyDoc.Load(xmlReader);
                            return xmlReplyDoc;
                        }  
                    }
                    if(response.ContentType == "")
                    {
                        string result;

                        StreamReader reader = new StreamReader(response.GetResponseStream());
                        result = reader.ReadToEnd();

                        throw new Exception(ExceptionMessage(response, result));
                    }
                    else
                    {
                        throw new Exception(ExceptionMessage(response));
                    }                    
                }                    
            }            
            else
            {
                throw new Exception(ExceptionMessage(response));
            }            
        }

        private string ExceptionMessage(HttpWebResponse response, string description = "")
        {
            return String.Format("Failed to get a valid XML response: {0}. Status code: {1}; Status message: {2}; Description: {3};",
                    response.ResponseUri,
                    response.StatusCode,
                    response.StatusDescription,
                    description);
        }

        private string ReadResponseStream(Stream responseStream)
        {
            System.Text.Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader readStream = new StreamReader(responseStream, encode);

            Char[] read = new Char[256];
            int count = readStream.Read(read, 0, 256);

            string webResponse = "";

            webResponse = ("HTML ...\r\n");
            while(count > 0)
            {
                String str = new String(read, 0, count);
                webResponse += str;
                count = readStream.Read(read, 0, 256);
            }

            webResponse += "";

            readStream.Close();

            return webResponse;
        }
    }
}
