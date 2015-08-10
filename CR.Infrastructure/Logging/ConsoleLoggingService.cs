using System;

namespace CR.Infrastructure.Logging
{
    public class ConsoleLoggingService : ILoggingService
    {
        private ConsoleColor _defaultColor = ConsoleColor.Gray;

        public void LogInfo(object logSource, string message, Exception exception = null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(string.Concat("Info from ", logSource.ToString(), ": ", message));
            PrintException(exception);
            ResetConsoleColor();
        }

        public void LogWarning(object logSource, string message, Exception exception = null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(string.Concat("Warning from ", logSource.ToString(), ": ", message));
            PrintException(exception);
            ResetConsoleColor();
        }
        public void LogError(object logSource, string message, Exception exception = null)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(string.Concat("Error from ", logSource.ToString(), ": ", message));
            PrintException(exception);
            ResetConsoleColor();
        }
        public void LogFatal(object logSource, string message, Exception exception = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(string.Concat("Fatal from ", logSource.ToString(), ": ", message));
            PrintException(exception);
            ResetConsoleColor();
        }

        public void LogDebug(object logSource, string message, Exception exception = null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(string.Concat("Debug from ", logSource.ToString(), ": ", message));
            PrintException(exception);
            ResetConsoleColor();
        }

        private void ResetConsoleColor()
        {
            Console.ForegroundColor = _defaultColor;
        }

        private void PrintException(Exception exception)
        {
            if (exception != null)
            {
                Console.WriteLine(string.Concat("Exception logged: ", exception.Message));
            }
        }
    }
}
