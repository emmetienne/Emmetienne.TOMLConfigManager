using Emmetienne.TOMLConfigManager.Constants;
using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Registries;
using Emmetienne.TOMLConfigManager.Repositories;
using Emmetienne.TOMLConfigManager.Services.Strategies;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;

namespace Emmetienne.TOMLConfigManager.Services
{
    public class TOMLConfigurationService
    {
        private readonly IOrganizationService sourceOrganizationService;
        private readonly IOrganizationService targetOrganizationService;
        private readonly ILogger logger;

        public TOMLConfigurationService(IOrganizationService sourceOrganizationService, IOrganizationService targetOrganizationService, ILogger logger)
        {
            this.sourceOrganizationService = sourceOrganizationService;
            this.targetOrganizationService = targetOrganizationService;
            this.logger = logger;
        }

        public List<TOMLOperationExecutable> PortConfigurations(List<TOMLOperationExecutable> TOMLOperationExecutableList, string baseSourceFilePath = null)
        {
            if (TOMLOperationExecutableList == null || TOMLOperationExecutableList.Count == 0)
            {
                logger.LogDebug("No TOML Operations to execute");
                return TOMLOperationExecutableList;
            }

            if (string.IsNullOrWhiteSpace(baseSourceFilePath))
                logger.LogWarning($"/!\\ No base path provided, using working directory as base path {Directory.GetCurrentDirectory()}");
            else
                logger.LogDebug($"Base path for files and images provided {baseSourceFilePath}");

            var repositoryRegistry = new RepositoryRegistry();

            repositoryRegistry.Add(RepositoryRegistryKeys.sourceRecordRepository, new D365RecordRepository(sourceOrganizationService));
            repositoryRegistry.Add(RepositoryRegistryKeys.targetRecordRepository, new D365RecordRepository(targetOrganizationService));
            repositoryRegistry.Add(RepositoryRegistryKeys.sourceEntityMetadataRepository, new EntityMetadataRepository(sourceOrganizationService));
            repositoryRegistry.Add(RepositoryRegistryKeys.targetEntityMetadataRepository, new EntityMetadataRepository(targetOrganizationService));
            repositoryRegistry.Add(RepositoryRegistryKeys.sourceFileRepository, new D365FileRepository(sourceOrganizationService));
            repositoryRegistry.Add(RepositoryRegistryKeys.targetFileRepository, new D365FileRepository(targetOrganizationService));

            logger.LogInfo("Starting TOML operations execution...");

            foreach (var operation in TOMLOperationExecutableList)
            {
                var strategy = OperationStrategyFactory.GetStrategy(operation.Type, logger);

                if (strategy != null)
                {
                    logger.LogDebug($"Executing {operation.Type} Operation for TOML");
                    logger.LogInfo(operation.ToString());

                    var operationExecutionContext = new OperationExecutionContext(operation, repositoryRegistry, baseSourceFilePath);

                    try
                    {
                        strategy.ExecuteOperation(operationExecutionContext);
                    }
                    catch (Exception ex)
                    {
                        operation.ErrorMessage = ex.Message;
                        logger.LogError($"Error executing operation: {ex.Message}");
                        continue;
                    }
                }
                else
                {
                    var errorMessage = $"No strategy found for operation of type {operation.Type}";
                    operation.ErrorMessage = errorMessage;
                    logger.LogError($"Error executing operation: {errorMessage}");
                    continue;
                }
            }

            return TOMLOperationExecutableList;
        }
    }
}