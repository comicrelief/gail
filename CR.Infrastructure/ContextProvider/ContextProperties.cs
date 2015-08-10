using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CR.Infrastructure.ContextProvider
{
    public class ContextProperties
    {
        private string _notAvailable = "N/A";
        public ContextProperties()
        {
            UserAgent = _notAvailable;
            RemoteHost = _notAvailable;
            Path = _notAvailable;
            Query = _notAvailable;
            Referrer = _notAvailable;
            RequestId = _notAvailable;
            SessionId = _notAvailable;
        }

        public string UserAgent { get; set; }
        public string RemoteHost { get; set; }
        public string Path { get; set; }
        public string Query { get; set; }
        public string Referrer { get; set; }
        public string RequestId { get; set; }
        public string SessionId { get; set; }
        public string Method { get; set; }
    }
}
