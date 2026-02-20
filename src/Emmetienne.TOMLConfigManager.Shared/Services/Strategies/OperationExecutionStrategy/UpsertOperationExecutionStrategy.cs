using Emmetienne.TOMLConfigManager.Constants;
using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Managers;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Emmetienne.TOMLConfigManager.Utilities;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace Emmetienne.TOMLConfigManager.Services.Strategies.OperationExecutionStrategy
{
    internal class UpsertOperationExecutionStrategy : IOperationExecutionStrategy
    {
        private readonly ILogger logger;

        public UpsertOperationExecutionStrategy(ILogger logger)
        {
            this.logger = logger;
        }

        public void ExecuteOperation(OperationExecutionContext operationExecutionContext)
        {
            var sourceD365RecordRepository = operationExecutionContext.Repositories.Get<D365RecordRepository>(RepositoryRegistryKeys.sourceRecordRepository);
            var targetD365RecordRepository = operationExecutionContext.Repositories.Get<D365RecordRepository>(RepositoryRegistryKeys.targetRecordRepository);
            var operation = operationExecutionContext.OperationExecutable;

            var sourceRecord = GetSourceRecord(sourceD365RecordRepository, operation);
            
            if (sourceRecord == null)
                return;

            var targetRecordId = GetTargetRecordId(targetD365RecordRepository, operation);

            if (!string.IsNullOrWhiteSpace(operation.ErrorMessage))
                return;

            // create a deepclone of the source record to be used for upsert operation, this is to ensure that the source record remains unchanged for file and image field synchronization later
            var recordToUpsert = sourceRecord.DeepClone();

            var entityMetadataRepository = operationExecutionContext.Repositories.Get<EntityMetadataRepository>(RepositoryRegistryKeys.targetEntityMetadataRepository);
            var entityFieldsMetadata = MetadataManager.Instance.GetEntityFieldsMetadata(operation.Table, entityMetadataRepository);

            // check for null values if present set the field to be nulled
            foreach (var metadataAttribute in entityFieldsMetadata.Keys)
            {
                if (recordToUpsert.Attributes.ContainsKey(metadataAttribute))
                    continue;

                recordToUpsert[metadataAttribute] = null;
            }

            recordToUpsert = recordToUpsert.RemoveOutOfTheBoxAndExcludedFields(operation.IgnoreFields);

            if (targetRecordId == null)
                CreateNewRecord(targetD365RecordRepository, recordToUpsert, operation);
            else
                UpdateExistingRecord(targetD365RecordRepository, recordToUpsert, targetRecordId.Value, operation);

            SyncFileAndImageFields(operationExecutionContext, sourceRecord, recordToUpsert, operation, entityFieldsMetadata);
        }

        private Entity GetSourceRecord(D365RecordRepository sourceRepository, TOMLOperationExecutable operation)
        {
            var sourceRecords = sourceRepository.GetRecordFromEnvironment(operation.Table, operation.MatchOn, operation.Row, true);

            if (sourceRecords.Entities.Count == 0)
            {
                SetError(operation, "No matching record found in source environment");
                return null;
            }

            if (sourceRecords.Entities.Count > 1)
            {
                SetError(operation, "Multiple matching records found in source environment");
                return null;
            }

            return sourceRecords[0];
        }

        private Guid? GetTargetRecordId(D365RecordRepository targetRepository, TOMLOperationExecutable operation)
        {
            var targetRecords = targetRepository.GetRecordFromEnvironment(operation.Table, operation.MatchOn, operation.Row, false);

            if (targetRecords.Entities.Count > 1)
            {
                SetError(operation, "Multiple matching records found in target environment");
                return null;
            }

            if (targetRecords.Entities.Count == 0)
                return null;

            return targetRecords[0].Id;
        }

        private void CreateNewRecord(D365RecordRepository targetRepository, Entity recordToUpsert, TOMLOperationExecutable operation)
        {
            logger.LogDebug("No matching record found in target environment. Creating new record.");

            var createdRecordId = targetRepository.CreateRecord(recordToUpsert);
            recordToUpsert.Id = createdRecordId;

            logger.LogDebug($"Record created in table {operation.Table} with Id {createdRecordId}.");
        }

        private void UpdateExistingRecord(D365RecordRepository targetRepository, Entity recordToUpsert, Guid targetRecordId, TOMLOperationExecutable operation)
        {
            logger.LogDebug("Matching record found in target environment. Updating existing record.");

            recordToUpsert.Id = targetRecordId;
            recordToUpsert[$"{operation.Table}id"] = targetRecordId;

            targetRepository.UpdateRecord(recordToUpsert);

            logger.LogDebug($"Record in table {operation.Table} with Id {recordToUpsert.Id} updated successfully in target environment");
        }

        private void SyncFileAndImageFields(OperationExecutionContext context, Entity sourceRecord, Entity targetRecord, TOMLOperationExecutable operation, Dictionary<string, FieldMetadata> entityFieldMetadata)
        {
            logger.LogDebug($"Retrieving entity metadata for table {operation.Table} to check if there are any file or image fields to update.");

            var fileImageSyncHelper = new FileImageFieldSyncService(logger, context.Repositories);

            var warningMessages = fileImageSyncHelper.SyncFileAndImageFieldsFromSourceRecord(sourceRecord, targetRecord, operation.IgnoreFields, entityFieldMetadata);

            operation.WarningMessage = string.Join(Environment.NewLine, warningMessages);
        }

        private void SetError(TOMLOperationExecutable operation, string errorMessage)
        {
            operation.ErrorMessage = errorMessage;
            logger.LogError(errorMessage);
        }
    }
}