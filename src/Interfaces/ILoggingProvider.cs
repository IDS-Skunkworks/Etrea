namespace Kingdoms_of_Etrea.Interfaces
{
    internal interface ILoggingProvider
    {
        void LogMessage(string message, LogLevel level, bool writeToScreen);
    }
}
