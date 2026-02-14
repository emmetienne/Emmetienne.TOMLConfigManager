using Emmetienne.TOMLConfigManager.Constants;
using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Managers;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Microsoft.Xrm.Sdk.Metadata;
using System;

namespace Emmetienne.TOMLConfigManager.Services.Strategies.OperationExecutionStrategy
{
    internal class CreateOperationExecutionStrategy : IOperationExecutionStrategy
    {
        private readonly ILogger logger;
        public CreateOperationExecutionStrategy(ILogger logger)
        {
            this.logger = logger;
        }
        public void ExecuteOperation(OperationExecutionContext operationExecutionContext)
        {
            var targetD365RecordRepository = operationExecutionContext.Repositories.Get<D365RecordRepository>(RepositoryRegistryKeys.targetRecordRepository);

            var operation = operationExecutionContext.OperationExecutable;

            var entityBuilder = new EntityBuilder();

            var recordToCreate = entityBuilder.BuildEntity(operationExecutionContext, null, out string errorMessages);

            if (recordToCreate == null)
            {
                var errorMessage = $"Failed to build record to create for table {operation.Table}. Errors: {errorMessages}";
                logger.LogError(errorMessage);
                operation.ErrorMessage = errorMessage;
                return;
            }

            logger.LogDebug($"Creating record in table {operation.Table}.");

            var createdRecordId = targetD365RecordRepository.CreateRecord(recordToCreate);

            logger.LogDebug($"Record created in table {operation.Table} with Id {createdRecordId}.");

            var targetEntityMetadataRepository = operationExecutionContext.Repositories.Get<EntityMetadataRepository>(RepositoryRegistryKeys.targetEntityMetadataRepository);

            foreach (var field in operation.Fields)
            {
                var fieldMetadata = MetadataManager.Instance.GetAttributeType(operation.Table, field, targetEntityMetadataRepository);

                if (fieldMetadata.AttributeType != typeof(FileAttributeMetadata) && fieldMetadata.AttributeType != typeof(ImageAttributeMetadata))
                    continue;

                var warningMessage = $"/!\\ Record has been created with Id <{createdRecordId}>, but file operations are not supported for create operations.{Environment.NewLine}Please use a replace operation to set the <{field}> on the newly created record.";
                logger.LogWarning(warningMessage);
                operation.WarningMessage = warningMessage;
                operation.IsRetryable = false;
            }      
        }
    }
}