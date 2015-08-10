using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CR.Infrastructure.Logging
{
    public abstract class LogProviderContext
    {
        private static readonly string _nameDataSlot = "LogProvider";

        public static ILoggingService DefaultLogProviderContext = new ConsoleLoggingService();

        public static ILoggingService Current
        {
            get
            {
                ILoggingService logProviderContext = Thread.GetData(Thread.GetNamedDataSlot(_nameDataSlot)) as ILoggingService;
                if (logProviderContext == null)
                {
                    logProviderContext = LogProviderContext.DefaultLogProviderContext;
                    Thread.SetData(Thread.GetNamedDataSlot(_nameDataSlot), logProviderContext);
                }
                return logProviderContext;
            }
            set
            {
                Thread.SetData(Thread.GetNamedDataSlot(_nameDataSlot), value);
            }
        }
    }
}
