using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Pacx.Logger;
using Emmetienne.TOMLConfigManager.Services;
using Greg.Xrm.Command;
using Greg.Xrm.Command.Services.Connection;
using Greg.Xrm.Command.Services.Output;
using Tomlyn;

namespace Emmetienne.TOMLConfigManager.Pacx.Configurations
{
    public class TOMLConfigurationManagerCommandExecutor : ICommandExecutor<TOMLConfigurationManagerCommand>
    {
        private readonly ILogger pacxTOMLLogger;
        private readonly IOrganizationServiceRepository organizationServiceRepository;

        public TOMLConfigurationManagerCommandExecutor(IOutput output, IOrganizationServiceRepository organizationServiceRepository)
        {
            this.pacxTOMLLogger = new PacxTOMLogger(output);
            this.organizationServiceRepository = organizationServiceRepository;
        }

        public async Task<CommandResult> ExecuteAsync(TOMLConfigurationManagerCommand command, CancellationToken cancellationToken)
        {
            var tomlContent = string.Empty;

            if (string.IsNullOrWhiteSpace(command.TOMLConfigFilePath) && string.IsNullOrWhiteSpace(command.TOMLString))
                return CommandResult.Fail("No TOML operations provided. Exiting...");

            if (!string.IsNullOrWhiteSpace(command.TOMLConfigFilePath) && !string.IsNullOrWhiteSpace(command.TOMLString))
                return CommandResult.Fail("Both TOML file and string has been provided, chose one or another. Exiting...");

            if (!string.IsNullOrWhiteSpace(command.TOMLString))
                tomlContent = command.TOMLString;
            else
                tomlContent = File.ReadAllText(command.TOMLConfigFilePath);

            pacxTOMLLogger.LogDebug("Provided TOML content:");
            pacxTOMLLogger.LogInfo(tomlContent);

            pacxTOMLLogger.LogInfo("Parsing TOML file...");
            var TOMLOperationsDeserialized = Toml.ToModel<TOMLParsed>(tomlContent);

            pacxTOMLLogger.LogDebug($"Connecting to source ({command.SourceConnection}) organization...");
            var sourceService = await organizationServiceRepository.GetConnectionByName(command.SourceConnection);

            pacxTOMLLogger.LogDebug($"Connecting to target ({command.TargetConnection}) organization...");
            var targetService = await organizationServiceRepository.GetConnectionByName(command.TargetConnection);

            var TOMLParsingService = new TOMLParsingService();

            var tomlOperationList = TOMLParsingService.ParseToTOMLExecutables(tomlContent, pacxTOMLLogger);

            if (tomlOperationList == null || tomlOperationList.Count == 0)
                return CommandResult.Fail("No TOML operations to execute. Exiting...");

            var tomlExecutionService = new TOMLConfigurationService(sourceService, targetService, pacxTOMLLogger);

            var tomlExecuted = tomlExecutionService.PortConfigurations(tomlOperationList);

            return CommandResult.Success();
        }
    }
}