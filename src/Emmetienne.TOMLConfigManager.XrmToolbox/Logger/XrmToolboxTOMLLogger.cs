using Emmetienne.TOMLConfigManager.Eventbus;

namespace Emmetienne.TOMLConfigManager.Logger
{
    public class XrmToolboxTOMLLogger : ILogger
    {
        public void LogInfo(string message)
        {
            var tmpLog = new LogModel();
            tmpLog.Message = message;
            tmpLog.Color = System.Drawing.Color.Black;
            tmpLog.LogLevel = LogLevel.info;

            WriteLog(tmpLog);
        }

        public void LogWarning(string message)
        {
            var tmpLog = new LogModel();
            tmpLog.Message = message;
            tmpLog.Color = System.Drawing.Color.Goldenrod;
            tmpLog.LogLevel = LogLevel.warning;

            WriteLog(tmpLog);
        }

        public void LogDebug(string message)
        {
            var tmpLog = new LogModel();
            tmpLog.Message = message;
            tmpLog.Color = System.Drawing.Color.Blue;
            tmpLog.LogLevel = LogLevel.debug;

            WriteLog(tmpLog);
        }

        public void LogError(string message)
        {
            var tmpLog = new LogModel();
            tmpLog.Message = message;
            tmpLog.Color = System.Drawing.Color.Red;
            tmpLog.LogLevel = LogLevel.error;

            WriteLog(tmpLog);
        }

        private void WriteLog(LogModel log)
        {
            EventbusSingleton.Instance.writeLog?.Invoke(log);
        }
    }
}
