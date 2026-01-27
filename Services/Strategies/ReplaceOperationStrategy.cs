using Emmetienne.TOMLConfigManager.Managers;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;

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

                var attributeType = MetadataManager.Instance.GetAttributeTypeCode(operation.Table, operation.Fields[i], targetEntityMetadataRepository);

                if (attributeType == null)
                {
                    recordToUpdate[operation.Fields[i]] = operation.Values[i];
                    continue;
                }

                if (attributeType == AttributeTypeCode.Picklist || attributeType == AttributeTypeCode.State || attributeType == AttributeTypeCode.Status)
                {
                    if (!int.TryParse(operation.Values[i], out int parsedInt))
                    {
                        operation.ErrorMessage = $"Value {operation.Values[i]} is not a valid int to be used for an Optionset value";
                        continue;
                    }

                    recordToUpdate[operation.Fields[i]] = new OptionSetValue(parsedInt);
                }

                if (attributeType == AttributeTypeCode.Lookup)
                {
                    if (!Guid.TryParse(operation.Values[i], out Guid parsedGuid))
                    {
                        operation.ErrorMessage = $"Value {operation.Values[i]} is not a valid GUID to be used for an entity reference";
                        continue;
                    }

                    recordToUpdate[operation.Fields[i]] = new EntityReference(operation.Table, parsedGuid);
                }

                if (attributeType == AttributeTypeCode.Integer || attributeType == AttributeTypeCode.BigInt)
                {
                    if (!int.TryParse(operation.Values[i], out int parsedInt))
                    {
                        operation.ErrorMessage = $"Value {operation.Values[i]} is not a valid integer";
                        continue;
                    }

                    recordToUpdate[operation.Fields[i]] = parsedInt;
                }

                if (attributeType == AttributeTypeCode.Uniqueidentifier)
                {
                    if (!Guid.TryParse(operation.Values[i], out Guid parsedGuid))
                    {
                        operation.ErrorMessage = $"Value {operation.Values[i]} is not a valid GUID";
                        continue;
                    }

                    recordToUpdate[operation.Fields[i]] = parsedGuid;
                }

                if (attributeType == AttributeTypeCode.Decimal || attributeType == AttributeTypeCode.Double || attributeType == AttributeTypeCode.Money)
                {
                    if (!double.TryParse(operation.Values[i], out double parsedDouble))
                    {
                        operation.ErrorMessage = $"Value {operation.Values[i]} is not a valid double";
                        continue;
                    }

                    recordToUpdate[operation.Fields[i]] = parsedDouble;
                }

                if (attributeType == AttributeTypeCode.Boolean)
                {
                    if (!bool.TryParse(operation.Values[i], out bool parsedBool))
                    {
                        operation.ErrorMessage = $"Value {operation.Values[i]} is not a valid bool";
                        continue;
                    }

                    recordToUpdate[operation.Fields[i]] = parsedBool;
                }

                recordToUpdate[operation.Fields[i]] = operation.Values[i];
            }

            targetD365RecordRepository.UpdateRecord(recordToUpdate);
        }
    }
}