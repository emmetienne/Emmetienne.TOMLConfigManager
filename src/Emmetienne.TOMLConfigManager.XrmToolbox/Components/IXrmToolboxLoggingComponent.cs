using Emmetienne.TOMLConfigManager.Logger;

namespace Emmetienne.TOMLConfigManager.Components
{
    public interface IXrmToolboxLoggingComponent
    {
        void WriteLog(LogModel log);
        void ClearLogs();
    }
}
