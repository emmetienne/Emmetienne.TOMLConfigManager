using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Services;
using Greg.Xrm.Command;
using Greg.Xrm.Command.Services.Connection;
using Greg.Xrm.Command.Services.Output;
using Tomlyn;

namespace Emmetienne.TOMLConfigManager.Pacx.Configurations
{
    public class TOMLConfigurationManagerCommandExecutor : ICommandExecutor<TOMLConfigurationManagerCommand>
    {
        private readonly IOutput output;
        private readonly IOrganizationServiceRepository organizationServiceRepository;

        public TOMLConfigurationManagerCommandExecutor(IOutput output, IOrganizationServiceRepository organizationServiceRepository)
        {
            this.output = output;
            this.organizationServiceRepository = organizationServiceRepository;
        }

        public async Task<CommandResult> ExecuteAsync(TOMLConfigurationManagerCommand command, CancellationToken cancellationToken)
        {
            //read from the parameter "path" the toml file
            var tomlContent = File.ReadAllText(command.TOMLConfigFilePath);

            output.WriteLine("TOML file content:");
            output.WriteLine(tomlContent);

            output.WriteLine("Parsing TOML file...");
            var TOMLOperationsDeserialized = Toml.ToModel<TOMLParsed>(tomlContent);

            output.WriteLine("Connecting to source organization...");
            var sourceService = await organizationServiceRepository.GetConnectionByName(command.SourceConnection);

            output.WriteLine("Connecting to target organization...");
            var targetService = await organizationServiceRepository.GetConnectionByName(command.TargetConnection);

            var TOMLParsingService = new TOMLParsingService();

            var tomlOperationList = TOMLParsingService.ParseToTOMLExecutables(tomlContent);

            var tomlExecutionService = new TOMLConfigurationService(sourceService, targetService);

            var tomlExecuted = tomlExecutionService.PortConfiguration(tomlOperationList);

            for (int i = 0; i < tomlExecuted.Count; i++)
            {
                output.WriteLine($"TOML #{i} {tomlExecuted[i].ErrorMessage}", tomlExecuted[i].Success ? ConsoleColor.Green : ConsoleColor.Red);
            }

            return CommandResult.Success();
        }
    }
}