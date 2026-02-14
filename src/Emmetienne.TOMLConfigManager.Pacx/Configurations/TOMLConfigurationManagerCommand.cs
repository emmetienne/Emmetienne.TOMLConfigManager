using Greg.Xrm.Command;

namespace Emmetienne.TOMLConfigManager.Pacx.Configurations
{
    [Command("configurations", "TOML", HelpText = "Manage configuration trought TOML configuration files")]
    public class TOMLConfigurationManagerCommand
    {
        [Option("path", "p", HelpText = "The path for the TOML file")]
        public string? TOMLConfigFilePath { get; set; }

        [Option("TOMLstring", "ts", HelpText = "The TOML string")]
        public string? TOMLString { get; set; }

        [Option("filebasepath", "fbp", HelpText = "The base path that will be used for retrieving files")]
        public string? FileBasePath { get; set; }

        [Option("source", "s", HelpText = "Name of the source connection to a D365 environment")]
        public string SourceConnection { get; set; }

        [Option("target", "t", HelpText = "Name of the source connection to a D365 environment")]
        public string TargetConnection { get; set; }
    }
}
