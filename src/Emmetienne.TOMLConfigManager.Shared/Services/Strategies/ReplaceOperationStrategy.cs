using Emmetienne.TOMLConfigManager.Constants;
using Emmetienne.TOMLConfigManager.Converters;
using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Managers;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Microsoft.Xrm.Sdk;

namespace Emmetienne.TOMLConfigManager.Services.Strategies
{
    internal class ReplaceOperationStrategy : IOperationStrategy
    {
        private readonly ILogger logger;

        public ReplaceOperationStrategy(ILogger logger)
        {
            this.logger = logger;
        }

        public void ExecuteOperation(OperationExecutionContext operationExecutionContext)
        {
            var targetD365RecordRepository = operationExecutionContext.Repositories.Get<D365RecordRepository>("Target.RecordRepository");

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

            var recordToUpdate = new Entity(operation.Table);
            recordToUpdate.Id = targetRecords.Entities[0].Id;


            for (int i = 0; i < operation.Fields.Count; i++)
            {
                var targetEntityMetadataRepository = operationExecutionContext.Repositories.Get<EntityMetadataRepository>(RepositoryRegistryKeys.targetEntityMetadataRepository);

                var fieldMetadata = MetadataManager.Instance.GetAttributeTypeCode(operation.Table, operation.Fields[i], targetEntityMetadataRepository);

                if (fieldMetadata == null)
                {
                    var errorMessage = $"Metadata not found for {operation.Fields[i]} in table {operation.Table}";
                    logger.LogError(errorMessage);
                    operation.ErrorMessage = errorMessage;
                    return;
                }

                recordToUpdate[operation.Fields[i]] = FieldValueConverter.Convert(operation.Values[i], fieldMetadata);
            }

            logger.LogDebug($"Updating record in table {operation.Table} with Id {recordToUpdate.Id} in target environment");

            targetD365RecordRepository.UpdateRecord(recordToUpdate);

            logger.LogDebug($"Record in table {operation.Table} with Id {recordToUpdate.Id} updated successfully in target environment");
        }
    }
}