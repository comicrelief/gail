using System;
using System.Threading;
using System.IO;

namespace CR.Infrastructure.ContextProvider
{
    public class ThreadContextService : IContextService
    {
        public string GetContextualFullFilePath(string filename)
        {
            string dir = Directory.GetCurrentDirectory();
            FileInfo resourceFileInfo = new FileInfo(Path.Combine(dir, filename));
            return resourceFileInfo.FullName;
        }
        public string GetUserName()
        {
            string username = "<null>";
            try
            {
                if (Thread.CurrentPrincipal != null)
                {
                    username = (Thread.CurrentPrincipal.Identity.IsAuthenticated
                        ? Thread.CurrentPrincipal.Identity.Name
                        : "<null>");
                }
            }
            catch { }
            return username;
        }
        public ContextProperties GetContextProperties()
        {
            return new ContextProperties();
        }
    }
}
