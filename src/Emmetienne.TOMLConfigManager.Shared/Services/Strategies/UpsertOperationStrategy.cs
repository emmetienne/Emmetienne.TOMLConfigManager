using Emmetienne.TOMLConfigManager.Constants;
using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Managers;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Emmetienne.TOMLConfigManager.Utilities;
using Microsoft.Xrm.Sdk.Metadata;

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
            var sourceD365RecordRepository = operationExecutionContext.Repositories.Get<D365RecordRepository>(RepositoryRegistryKeys.sourceRecordRepository);
            var targetD365RecordRepository = operationExecutionContext.Repositories.Get<D365RecordRepository>(RepositoryRegistryKeys.targetRecordRepository);
            var entityMetadataRepository = operationExecutionContext.Repositories.Get<EntityMetadataRepository>(RepositoryRegistryKeys.targetEntityMetadataRepository);

            var operation = operationExecutionContext.OperationExecutable;

            var sourceRecords = sourceD365RecordRepository.GetRecordFromEnvironment(operation.Table, operation.MatchOn, operation.Row, true);

            if (sourceRecords.Entities.Count == 0)
            {
                var errorMessage = "No matching record found in source environment";
                operation.ErrorMessage = errorMessage;
                logger.LogError(errorMessage);
                return;
            }

            if (sourceRecords.Entities.Count > 1)
            {
                var errorMessage = "Multiple matching records found in source environment";
                operation.ErrorMessage = errorMessage;
                logger.LogError(errorMessage);
                return;
            }

            var targetRecords = targetD365RecordRepository.GetRecordFromEnvironment(operation.Table, operation.MatchOn, operation.Row, false);

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

            logger.LogDebug($"Retrieving entity metadata for table {operation.Table} to check if there are any file or image fields to update.");

            var allEntityFiledsMetadata = MetadataManager.Instance.GetEntityFieldsMetadata(operation.Table, entityMetadataRepository);

            var sourceFileAttributeRepository = operationExecutionContext.Repositories.Get<D365FileRepository>(RepositoryRegistryKeys.sourceFileRepository);
            var targetFileAttributeRepository = operationExecutionContext.Repositories.Get<D365FileRepository>(RepositoryRegistryKeys.targetFileRepository);

            foreach (var fieldKey in allEntityFiledsMetadata.Keys)
            {
                var tmpFieldMetadata = allEntityFiledsMetadata[fieldKey].AttributeType;


                if (tmpFieldMetadata != typeof(FileAttributeMetadata) && tmpFieldMetadata != typeof(ImageAttributeMetadata))
                    continue;

                // per ora così, una cosa alla volta
                if (tmpFieldMetadata == typeof(ImageAttributeMetadata))
                    continue;

                // da separare poi in un service dedicato, per ora lo lascio qui
                var sourceFile = sourceFileAttributeRepository.DownloadFile(sourceRecords[0].ToEntityReference(), fieldKey);

                var fileName = sourceRecords[0].Attributes[$"{fieldKey}_name"].ToString();

                targetFileAttributeRepository.UploadFile(sourceFile, recordToUpsert.ToEntityReference(), fieldKey, fileName);
            }
        }
    }
}