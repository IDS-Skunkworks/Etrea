namespace Etrea2.Interfaces
{
    internal interface ILoggingProvider
    {
        void LogMessage(string message, LogLevel level, bool writeToScreen);
    }
}
