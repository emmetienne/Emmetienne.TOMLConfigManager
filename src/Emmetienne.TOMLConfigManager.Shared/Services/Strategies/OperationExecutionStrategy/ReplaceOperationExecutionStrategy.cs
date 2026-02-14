using Emmetienne.TOMLConfigManager.Constants;
using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using System;

namespace Emmetienne.TOMLConfigManager.Services.Strategies.OperationExecutionStrategy
{
    internal class ReplaceOperationExecutionStrategy : IOperationExecutionStrategy
    {
        private readonly ILogger logger;

        public ReplaceOperationExecutionStrategy(ILogger logger)
        {
            this.logger = logger;
        }

        public void ExecuteOperation(OperationExecutionContext operationExecutionContext)
        {
            var targetD365RecordRepository = operationExecutionContext.Repositories.Get<D365RecordRepository>(RepositoryRegistryKeys.targetRecordRepository);

            var operation = operationExecutionContext.OperationExecutable;

            var targetRecords = targetD365RecordRepository.GetRecordFromEnvironment(operation.Table, operation.MatchOn, operation.Row, true);

            if (targetRecords.Entities.Count == 0)
            {
                var errorMessage = "No record to update found in target environment";
                logger.LogError(errorMessage);
                operation.ErrorMessage = errorMessage;
                return;
            }

            if (targetRecords.Entities.Count > 1)
            {
                var errorMessage = "Multiple matching records found in target environment";
                logger.LogError(errorMessage);
                operation.ErrorMessage = errorMessage;
                return;
            }

            var entityBuilder = new EntityBuilder();

            var recordToUpdate = entityBuilder.BuildEntity(operationExecutionContext, targetRecords.Entities[0].Id, out string errorMessages);

            if (recordToUpdate == null)
            {
                var errorMessage = $"Failed to build record to update for table {operation.Table} with Id {targetRecords.Entities[0].Id}. Errors: {errorMessages}";
                logger.LogError(errorMessage);
                operation.ErrorMessage = errorMessage;
                return;
            }

            logger.LogDebug($"Updating record in table {operation.Table} with Id {recordToUpdate.Id} in target environment");

            targetD365RecordRepository.UpdateRecord(recordToUpdate);

            logger.LogDebug($"Record in table {operation.Table} with Id {recordToUpdate.Id} updated successfully in target environment");

            var fileImageFieldSyncService = new FileImageFieldSyncService(logger, operationExecutionContext.Repositories);

            var warningMessageList = fileImageFieldSyncService.SyncFileAndImageFieldsFromConfiguration(operationExecutionContext, recordToUpdate.Id);

            operation.WarningMessage = string.Join(Environment.NewLine, warningMessageList);
        }
    }
}