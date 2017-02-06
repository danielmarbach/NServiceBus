namespace NServiceBus
{
    using Logging;

    static class LogLevelReader
    {
        public static LogLevel GetDefaultLogLevel(LogLevel fallback = LogLevel.Info)
        {
            return fallback;
        }
    }
}