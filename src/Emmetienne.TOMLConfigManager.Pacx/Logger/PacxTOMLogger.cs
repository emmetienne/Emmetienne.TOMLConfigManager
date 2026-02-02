using Emmetienne.TOMLConfigManager.Logger;
using Greg.Xrm.Command.Services.Output;

namespace Emmetienne.TOMLConfigManager.Pacx.Logger
{
    public class PacxTOMLogger : ILogger
    {
        private readonly IOutput output;
        public PacxTOMLogger(IOutput output)
        {
            this.output = output;
        }

        public void LogDebug(string message)
        {
            output.WriteLine(message,ConsoleColor.Blue);
        }

        public void LogError(string message)
        {
            output.WriteLine(message, ConsoleColor.Red);
        }
        

        public void LogInfo(string message)
        {
            output.WriteLine(message, ConsoleColor.White);
        }

        public void LogWarning(string message)
        {
            output.WriteLine(message, ConsoleColor.Yellow);
        }
    }
}
