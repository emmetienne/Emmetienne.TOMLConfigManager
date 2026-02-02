namespace Emmetienne.TOMLConfigManager.Logger
{
    public interface ILogger
    {
        void LogInfo(string message);
        void LogError(string message);
        void LogWarning(string message);
        void LogDebug(string message);
    }
}
