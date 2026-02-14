using Emmetienne.TOMLConfigManager.Constants;
using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;

namespace Emmetienne.TOMLConfigManager.Services.Strategies.OperationExecutionStrategy
{
    internal class DeleteOperationExecutionStrategy : IOperationExecutionStrategy
    {
        private readonly ILogger logger;

        public DeleteOperationExecutionStrategy(ILogger logger)
        {
            this.logger = logger;
        }

        public void ExecuteOperation(OperationExecutionContext operationExecutionContext)
        {
            var d365RecordRepository = operationExecutionContext.Repositories.Get<D365RecordRepository>(RepositoryRegistryKeys.targetRecordRepository);
            var operation = operationExecutionContext.OperationExecutable;

            var record = d365RecordRepository.GetRecordFromEnvironment(operation.Table, operation.MatchOn, operation.Row, false);

            if (record.Entities.Count == 0)
            {
                var errorMessage = $"No record found to delete on table '{operation.Table}' matching criteria.";
                logger.LogError(errorMessage);
                operation.ErrorMessage = errorMessage;
                return;
            }

            logger.LogDebug($"Deleting record on table '{operation.Table}' with ID '{record.Entities[0].Id}'.");

            d365RecordRepository.DeleteRecord(record.Entities[0].LogicalName, record.Entities[0].Id);

            logger.LogDebug($"Record on table '{operation.Table}' with ID '{record.Entities[0].Id}' deleted successfully.");
        }
    }
}