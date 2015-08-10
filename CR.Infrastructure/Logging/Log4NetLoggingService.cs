using System;
using System.IO;
using System.Threading.Tasks;
using log4net;

using CR.Infrastructure.Configuration;
using CR.Infrastructure.ContextProvider;

namespace CR.Infrastructure.Logging
{
    public class Log4NetLoggingService : ILoggingService
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IContextService _contextService;
        private string _log4netConfigFileName;

        public Log4NetLoggingService(IConfigurationRepository configurationRepository, IContextService contextService)
        {
            if (configurationRepository == null) throw new ArgumentNullException("ConfigurationRepository");
            if (contextService == null) throw new ArgumentNullException("ContextService");
            _configurationRepository = configurationRepository;
            _contextService = contextService;
            _log4netConfigFileName = configurationRepository.GetConfigurationValue<string>("Log4NetSettingsFile");

            if (string.IsNullOrEmpty(_log4netConfigFileName))
            {
                throw new ApplicationException("Log4net settings file missing from the configuration source.");
            }

            SetupLogger();
        }

        private void SetupLogger()
        {
            FileInfo log4netSettingsFileInfo = new FileInfo(_contextService.GetContextualFullFilePath(_log4netConfigFileName));
            if (!log4netSettingsFileInfo.Exists)
            {
                throw new ApplicationException(string.Concat("Log4net settings file ", _log4netConfigFileName, " not found."));
            }
            log4net.Config.XmlConfigurator.ConfigureAndWatch(log4netSettingsFileInfo);
        }

        public void LogInfo(object logSource, string message, Exception exception = null)
        {
            LogMessageWithProperties(logSource, message, log4net.Core.Level.Info, exception);
        }
        public void LogError(object logSource, string message, Exception exception = null)
        {
            LogMessageWithProperties(logSource, message, log4net.Core.Level.Error, exception);
        }

        public void LogFatal(object logSource, string message, Exception exception = null)
        {
            LogMessageWithProperties(logSource, message, log4net.Core.Level.Fatal, exception);
        }

        public void LogWarning(object logSource, string message, Exception exception = null)
        {
            LogMessageWithProperties(logSource, message, log4net.Core.Level.Warn, exception);
        }

        public void LogDebug(object logSource, string message, Exception exception = null)
        {
            LogMessageWithProperties(logSource, message, log4net.Core.Level.Debug, exception);
        }

        private void LogMessageWithProperties(object logSource, string message, log4net.Core.Level level, Exception exception)
        {
            var logger = log4net.LogManager.GetLogger(logSource.GetType());

            var loggingEvent = new log4net.Core.LoggingEvent(logSource.GetType(), logger.Logger.Repository, logger.Logger.Name, level, message, null);
            AddProperties(logSource, exception, loggingEvent);
            try
            {
                logger.Logger.Log(loggingEvent);
            }
            catch (AggregateException ae)
            {
                ae.Handle(x => { return true; });
            }
            catch (Exception) { }
        }

        private string GetUserName()
        {
            return _contextService.GetUserName();
        }

        private void AddProperties(object logSource, Exception exception, log4net.Core.LoggingEvent loggingEvent)
        {
            loggingEvent.Properties["UserName"] = GetUserName();
            try
            {
                ContextProperties contextProperties = _contextService.GetContextProperties();
                if (contextProperties != null)
                {
                    try
                    {
                        loggingEvent.Properties["UserAgent"] = contextProperties.UserAgent;
                        loggingEvent.Properties["RemoteHost"] = contextProperties.RemoteHost;
                        loggingEvent.Properties["Path"] = contextProperties.Path;
                        loggingEvent.Properties["Query"] = contextProperties.Query;
                        loggingEvent.Properties["RefererUrl"] = contextProperties.Referrer;
                        loggingEvent.Properties["RequestId"] = contextProperties.RequestId;
                        loggingEvent.Properties["SessionId"] = contextProperties.SessionId;
                    }
                    catch (Exception)
                    {
                    }
                }

                loggingEvent.Properties["ExceptionType"] = exception == null ? "" : exception.GetType().ToString();
                loggingEvent.Properties["ExceptionMessage"] = exception == null ? "" : exception.Message;
                loggingEvent.Properties["ExceptionStackTrace"] = exception == null ? "" : exception.StackTrace;
                loggingEvent.Properties["LogSource"] = logSource.GetType().ToString();
            }
            catch (Exception ex)
            {
                var type = typeof(Log4NetLoggingService);
                var logger = LogManager.GetLogger(type);
                logger.Logger.Log(type, log4net.Core.Level.Fatal, "Exception when extracting properties: " + ex.Message, ex);
            }
        }
    }
}
