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
        private IOrganizationService sourceOrganizationService;
        private IOrganizationService targetOrganizationService;

        public TOMLConfigurationService(IOrganizationService sourceOrganizationService, IOrganizationService targetOrganizationService)
        {
            this.sourceOrganizationService = sourceOrganizationService;
            this.targetOrganizationService = targetOrganizationService;
        }

        public List<TOMLOperationExecutable> PortConfiguration(List<TOMLOperationExecutable> TOMLOperationExecutableList)
        {
            var repositoryRegistry = new RepositoryRegistry();

            repositoryRegistry.Add("Source.RecordRepository", new D365RecordRepository(sourceOrganizationService));
            repositoryRegistry.Add("Target.RecordRepository", new D365RecordRepository(targetOrganizationService));
            repositoryRegistry.Add("Source.EntityMetadataRepository", new EntityMetadataRepository(sourceOrganizationService));
            repositoryRegistry.Add("Target.EntityMetadataRepository", new EntityMetadataRepository(targetOrganizationService));

            foreach (var operation in TOMLOperationExecutableList)
            {
                var strategy = OperationStrategyFactory.GetStrategy(operation.Type);

                if (strategy != null)
                {
                    var operationExecutionContext = new OperationExecutionContext(operation, repositoryRegistry);
                    try
                    {
                        strategy.ExecuteOperation(operationExecutionContext);
                    }
                    catch (Exception ex)
                    {
                        operation.ErrorMessage = ex.Message;
                        continue;
                    }
                }
            }

            return TOMLOperationExecutableList;
        }
    }
}