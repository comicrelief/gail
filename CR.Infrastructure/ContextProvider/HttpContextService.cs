using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CR.Infrastructure.ContextProvider
{
    public class HttpContextService : IContextService
    {
        public HttpContextService()
        {
            if(HttpContext.Current == null)
            {
                throw new ArgumentNullException("There is no HttpContext available");
            }
        }
        public string GetContextualFullFilePath(string fileName)
        {
            return HttpContext.Current.Server.MapPath(string.Concat("~/", fileName));
        }

        public string GetUserName()
        {
            string userName = "<null>";
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.User != null)
                {
                    userName = (HttpContext.Current.User.Identity.IsAuthenticated
                                    ? HttpContext.Current.User.Identity.Name
                                    : "<null>");
                }
            }
            catch
            {
            }
            return userName;
        }

        public ContextProperties GetContextProperties()
        {
            ContextProperties props = new ContextProperties();
        if (HttpContext.Current != null)
        {
            HttpRequest request = null;
            try
            {
                request = HttpContext.Current.Request;
            }
            catch (HttpException)
            {
            }
                if (request != null)
            {
                props.UserAgent = request.Browser == null ? "" : request.Browser.Browser;
                props.RemoteHost = request.ServerVariables == null ? "" : request.ServerVariables["REMOTE_HOST"];
                props.Path = request.Url == null ? "" : request.Url.AbsolutePath;
                props.Query = request.Url == null ? "" : request.Url.Query;
                props.Referrer = request.UrlReferrer == null ? "" : request.UrlReferrer.ToString();
                props.Method = request.HttpMethod;
            }

            //var items = HttpContext.Current.Items;
            IDictionary items = HttpContext.Current.Items;
            if (items != null)
            {
                var requestId = items["RequestId"];
                if (requestId != null)
                {
                    props.RequestId = items["RequestId"].ToString();
                }
            }

            var session = HttpContext.Current.Session;
            if (session != null)
            {
                var sessionId = session["SessionId"];
                if (sessionId != null)
                {
                    props.SessionId = session["SessionId"].ToString();
                }
            }
        }

        return props;
        }
    }
}
