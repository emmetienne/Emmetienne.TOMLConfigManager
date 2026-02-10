using Emmetienne.TOMLConfigManager.Constants;
using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Managers;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Emmetienne.TOMLConfigManager.Utilities;
using Microsoft.Xrm.Sdk;
using System;

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
            var operation = operationExecutionContext.OperationExecutable;

            var sourceRecord = GetValidatedSourceRecord(sourceD365RecordRepository, operation);
            if (sourceRecord == null)
                return;

            var targetRecordId = GetValidatedTargetRecordId(targetD365RecordRepository, operation);
            if (targetRecordId == null && operation.ErrorMessage != null)
                return;

            var recordToUpsert = sourceRecord.RemoveOutOfTheBoxAndExcludedFields(operation.IgnoreFields);

            if (targetRecordId == null)
            {
                CreateNewRecord(targetD365RecordRepository, recordToUpsert, operation);
            }
            else
            {
                UpdateExistingRecord(targetD365RecordRepository, recordToUpsert, targetRecordId.Value, operation);
            }

            SyncFileAndImageFields(operationExecutionContext, sourceRecord, recordToUpsert, operation);
        }

        private Entity GetValidatedSourceRecord(D365RecordRepository sourceRepository, TOMLOperationExecutable operation)
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

        private Guid? GetValidatedTargetRecordId(D365RecordRepository targetRepository, TOMLOperationExecutable operation)
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

            logger.LogDebug($"Record in table {operation.Table} with ID {recordToUpsert.Id} updated successfully in target environment");
        }

        private void SyncFileAndImageFields(OperationExecutionContext context, Entity sourceRecord, Entity targetRecord, TOMLOperationExecutable operation)
        {
            logger.LogDebug($"Retrieving entity metadata for table {operation.Table} to check if there are any file or image fields to update.");

            var entityMetadataRepository = context.Repositories.Get<EntityMetadataRepository>(RepositoryRegistryKeys.targetEntityMetadataRepository);
            var allEntityFieldsMetadata = MetadataManager.Instance.GetEntityFieldsMetadata(operation.Table, entityMetadataRepository);

            var targetD365RecordRepository = context.Repositories.Get<D365RecordRepository>(RepositoryRegistryKeys.targetRecordRepository);
            var sourceFileRepository = context.Repositories.Get<D365FileRepository>(RepositoryRegistryKeys.sourceFileRepository);
            var targetFileRepository = context.Repositories.Get<D365FileRepository>(RepositoryRegistryKeys.targetFileRepository);

            var fileImageSyncHelper = new FileImageFieldSyncHelper(logger,targetD365RecordRepository,sourceFileRepository,targetFileRepository);

            var warningMessages = fileImageSyncHelper.SyncFileAndImageFields(sourceRecord, targetRecord, allEntityFieldsMetadata);

            operation.WarningMessage = string.Join(Environment.NewLine, warningMessages);
        }

        private void SetError(TOMLOperationExecutable operation, string errorMessage)
        {
            operation.ErrorMessage = errorMessage;
            logger.LogError(errorMessage);
        }
    }
}