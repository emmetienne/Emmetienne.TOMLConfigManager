using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Emmetienne.TOMLConfigManager.Utilities;
using Microsoft.Xrm.Sdk;

namespace Emmetienne.TOMLConfigManager.Services.Strategies
{
    internal class UpsertOperationStrategy : IOperationStrategy
    {
        public void ExecuteOperation(OperationExecutionContext operationExecutionContext)
        {
            var sourceD365RecordRepository = operationExecutionContext.Repositories.Get<D365RecordRepository>("Source.RecordRepository");
            var targetD365RecordRepository = operationExecutionContext.Repositories.Get<D365RecordRepository>("Target.RecordRepository");

            var operation = operationExecutionContext.OperationExecutable;

            var sourceRecords = sourceD365RecordRepository.GetRecordFromEnvironment(operation.Table, operation.MatchOn, operation.Row, true);

            if (sourceRecords.Entities.Count > 1)
            {
                operation.ErrorMessage = ("Multiple matching records found in source environment");
                return;
            }

            var targetRecords = targetD365RecordRepository.GetRecordFromEnvironment(operation.Table, operation.MatchOn, operation.Row, true);

            if (targetRecords.Entities.Count > 1)
            {
                operation.ErrorMessage = ("Multiple matching records found in target environment");
                return;
            }

            var recordToUpsert = sourceRecords[0];
            recordToUpsert = recordToUpsert.RemoveOutOfTheBoxAndExcludedFields(operation.IgnoreFields);

            if (targetRecords.Entities.Count == 0)
            {
                targetD365RecordRepository.CreateRecord(recordToUpsert);
            }
            else
            {
                recordToUpsert.Id = targetRecords[0].Id;
                recordToUpsert[$"{operation.Table}id"] = targetRecords[0].Id;

                targetD365RecordRepository.UpdateRecord(recordToUpsert);
            }
        }
    }
}