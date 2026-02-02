using Emmetienne.TOMLConfigManager.Converters;
using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Managers;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Microsoft.Xrm.Sdk;

namespace Emmetienne.TOMLConfigManager.Services.Strategies
{
    internal class CreateOperationStrategy : IOperationStrategy
    {
        private readonly ILogger logger;
        public CreateOperationStrategy(ILogger logger)
        {
            this.logger = logger;
        }
        public void ExecuteOperation(OperationExecutionContext operationExecutionContext)
        {
            var targetD365RecordRepository = operationExecutionContext.Repositories.Get<D365RecordRepository>("Target.RecordRepository");

            var operation = operationExecutionContext.OperationExecutable;

            var recordToCreate = new Entity(operation.Table);

            // gestione cache
            for (int i = 0; i < operation.Fields.Count; i++)
            {
                var targetEntityMetadataRepository = operationExecutionContext.Repositories.Get<EntityMetadataRepository>("Target.EntityMetadataRepository");

                var fieldMetadata = MetadataManager.Instance.GetAttributeTypeCode(operation.Table, operation.Fields[i], targetEntityMetadataRepository);

                recordToCreate[operation.Fields[i]] = FieldValueConverter.Convert(operation.Values[i], fieldMetadata);
            }

            logger.LogDebug($"Creating record in table {operation.Table}.");

            var createdRecordId = targetD365RecordRepository.CreateRecord(recordToCreate);

            logger.LogDebug($"Record created in table {operation.Table} with Id {createdRecordId}.");
        }
    }
}