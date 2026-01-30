using Greg.Xrm.Command;

namespace Emmetienne.TOMLConfigManager.Pacx.Configurations
{
    [Command("configurations", "TOML", HelpText = "Manage configuration trought TOML configuration files")]
    public class TOMLConfigurationManagerCommand
    {
        [Option("path", "p", HelpText = "The path for the TOML file")]
        public string TOMLConfigFilePath { get; set; }

        [Option("source", "s", HelpText = "Name of the source connection to a D365 environment")]
        public string SourceConnection { get; set; }

        [Option("target", "t", HelpText = "Name of the source connection to a D365 environment")]
        public string TargetConnection { get; set; }
    }
}
