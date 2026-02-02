using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Emmetienne.TOMLConfigManager.Utilities;

namespace Emmetienne.TOMLConfigManager.Services.Strategies
{
    internal class UpsertOperationStrategy : IOperationStrategy
    {
        private readonly ILogger logger;

        public UpsertOperationStrategy(ILogger logger)
        {
            this.logger = logger;
        }

        public void ExecuteOperation(OperationExecutionContext operationExecutionContext)
        {
            var sourceD365RecordRepository = operationExecutionContext.Repositories.Get<D365RecordRepository>("Source.RecordRepository");
            var targetD365RecordRepository = operationExecutionContext.Repositories.Get<D365RecordRepository>("Target.RecordRepository");

            var operation = operationExecutionContext.OperationExecutable;



            var sourceRecords = sourceD365RecordRepository.GetRecordFromEnvironment(operation.Table, operation.MatchOn, operation.Row, true);

            if (sourceRecords.Entities.Count > 1)
            {
                var errorMessage = "Multiple matching records found in source environment";
                operation.ErrorMessage = errorMessage;
                logger.LogError(errorMessage);
                return;
            }

            var targetRecords = targetD365RecordRepository.GetRecordFromEnvironment(operation.Table, operation.MatchOn, operation.Row, true);

            if (targetRecords.Entities.Count > 1)
            {
                var errorMessage = "Multiple matching records found in target environment";
                operation.ErrorMessage = errorMessage;
                logger.LogError(errorMessage);
                return;
            }

            var recordToUpsert = sourceRecords[0];
            recordToUpsert = recordToUpsert.RemoveOutOfTheBoxAndExcludedFields(operation.IgnoreFields);

            if (targetRecords.Entities.Count == 0)
            {
                logger.LogDebug("No matching record found in target environment. Creating new record.");

                var createdRecordId = targetD365RecordRepository.CreateRecord(recordToUpsert);

                logger.LogDebug($"Record created in table {operation.Table} with Id {createdRecordId}.");
            }
            else
            {
                logger.LogDebug("Matching record found in target environment. Updating existing record.");

                recordToUpsert.Id = targetRecords[0].Id;
                recordToUpsert[$"{operation.Table}id"] = targetRecords[0].Id;

                targetD365RecordRepository.UpdateRecord(recordToUpsert);

                logger.LogDebug($"Record in table {operation.Table} with ID {recordToUpsert.Id} updated successfully in target environment");

            }
        }
    }
}