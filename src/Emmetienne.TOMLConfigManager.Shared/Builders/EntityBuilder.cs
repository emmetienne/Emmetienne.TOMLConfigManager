using Emmetienne.TOMLConfigManager.Constants;
using Emmetienne.TOMLConfigManager.Converters;
using Emmetienne.TOMLConfigManager.Managers;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;

namespace Emmetienne.TOMLConfigManager.Services
{
    public class EntityBuilder
    {
        public Entity BuildEntity(OperationExecutionContext context, Guid? recordId, out string errorMessages)
        {
            errorMessages = string.Empty;

            var entity = new Entity(context.OperationExecutable.Table);

            if (recordId.HasValue)
                entity.Id = recordId.Value;

            var targetEntityMetadataRepository = context.Repositories.Get<EntityMetadataRepository>(RepositoryRegistryKeys.targetEntityMetadataRepository);

            var operation = context.OperationExecutable;

            for (int i = 0; i < operation.Fields.Count; i++)
            {
                var fieldMetadata = MetadataManager.Instance.GetAttributeType(operation.Table, operation.Fields[i], targetEntityMetadataRepository);

                if (fieldMetadata == null)
                {
                    errorMessages += $"Metadata not found for {operation.Fields[i]} in table {operation.Table}{Environment.NewLine}";
                    continue;
                }

                // these attributes type will be handled separately in the file/image handling part, we just need to skip them for now
                if (fieldMetadata.AttributeType == typeof(FileAttributeMetadata) || fieldMetadata.AttributeType == typeof(ImageAttributeMetadata))
                    continue;

                entity[operation.Fields[i]] = FieldValueConverter.Convert(operation.Values[i], fieldMetadata);
            }

            if (!string.IsNullOrEmpty(errorMessages))
                return null;

            return entity;
        }
    }
}
