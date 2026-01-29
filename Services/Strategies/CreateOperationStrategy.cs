using Emmetienne.TOMLConfigManager.Converters;
using Emmetienne.TOMLConfigManager.Managers;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Microsoft.Xrm.Sdk;

namespace Emmetienne.TOMLConfigManager.Services.Strategies
{
    internal class CreateOperationStrategy : IOperationStrategy
    {
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

            targetD365RecordRepository.CreateRecord(recordToCreate);
        }
    }
}