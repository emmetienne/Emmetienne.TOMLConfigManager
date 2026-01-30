using Emmetienne.TOMLConfigManager.Converters;
using Emmetienne.TOMLConfigManager.Managers;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Microsoft.Xrm.Sdk;

namespace Emmetienne.TOMLConfigManager.Services.Strategies
{
    internal class ReplaceOperationStrategy : IOperationStrategy
    {
        public void ExecuteOperation(OperationExecutionContext operationExecutionContext)
        {
            var targetD365RecordRepository = operationExecutionContext.Repositories.Get<D365RecordRepository>("Target.RecordRepository");

            var operation = operationExecutionContext.OperationExecutable;

            var targetRecords = targetD365RecordRepository.GetRecordFromEnvironment(operation.Table, operation.MatchOn, operation.Row, true);

            if (targetRecords.Entities.Count == 0)
            {
                operation.ErrorMessage = ("No record to update found in target environment");
                return;
            }

            if (targetRecords.Entities.Count > 1)
            {
                operation.ErrorMessage = ("Multiple matching records found in target environment");
                return;
            }

            var recordToUpdate = new Entity(operation.Table);
            recordToUpdate.Id = targetRecords.Entities[0].Id;

            // gestione cache
            for (int i = 0; i < operation.Fields.Count; i++)
            {
                var targetEntityMetadataRepository = operationExecutionContext.Repositories.Get<EntityMetadataRepository>("Target.EntityMetadataRepository");

                var fieldMetadata = MetadataManager.Instance.GetAttributeTypeCode(operation.Table, operation.Fields[i], targetEntityMetadataRepository);


                recordToUpdate[operation.Fields[i]] = FieldValueConverter.Convert(operation.Values[i], fieldMetadata);
            }

            targetD365RecordRepository.UpdateRecord(recordToUpdate);
        }
    }
}