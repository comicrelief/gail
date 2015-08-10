using System;

namespace CR.Infrastructure.ContextProvider
{
    public interface IContextService
    {
        string GetContextualFullFilePath(string filename);
        string GetUserName();
        ContextProperties GetContextProperties();
    }
}
