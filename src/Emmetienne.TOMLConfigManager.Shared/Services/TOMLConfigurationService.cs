using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Registries;
using Emmetienne.TOMLConfigManager.Repositories;
using Emmetienne.TOMLConfigManager.Services.Strategies;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

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

        public List<TOMLOperationExecutable> PortConfiguration(List<TOMLOperationExecutable> TOMLOperationExecutableList)
        {
            var repositoryRegistry = new RepositoryRegistry();

            repositoryRegistry.Add("Source.RecordRepository", new D365RecordRepository(sourceOrganizationService));
            repositoryRegistry.Add("Target.RecordRepository", new D365RecordRepository(targetOrganizationService));
            repositoryRegistry.Add("Source.EntityMetadataRepository", new EntityMetadataRepository(sourceOrganizationService));
            repositoryRegistry.Add("Target.EntityMetadataRepository", new EntityMetadataRepository(targetOrganizationService));

            logger.LogInfo("Starting TOML operations execution...");

            foreach (var operation in TOMLOperationExecutableList)
            {
                var strategy = OperationStrategyFactory.GetStrategy(operation.Type, logger);

                if (strategy != null)
                {
                    logger.LogDebug($"Executing {operation.Type} Operation for TOML");
                    logger.LogInfo(operation.ToString());

                    var operationExecutionContext = new OperationExecutionContext(operation, repositoryRegistry);

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
            }

            return TOMLOperationExecutableList;
        }
    }
}