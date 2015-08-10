using System;
using System.Collections.Specialized;

namespace CR.Infrastructure.Configuration
{
    public interface IConfigurationRepository
    {
        T GetConfigurationValue<T>(string key);
        T GetConfigurationValue<T>(string key, T defaultValue);
        NameValueCollection AllEntries();
    }
}
