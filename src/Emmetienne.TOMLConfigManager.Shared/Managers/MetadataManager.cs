using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;

namespace Emmetienne.TOMLConfigManager.Managers
{
    public class MetadataManager : Singleton<MetadataManager>
    {
        private Dictionary<string, Dictionary<string, FieldMetadata>> metadataCache = new Dictionary<string, Dictionary<string, FieldMetadata>>();

        public Dictionary<string, FieldMetadata> GetEntityFieldsMetadata(string entityName, EntityMetadataRepository entityMetadataRepository)
        {
            if (metadataCache.ContainsKey(entityName))
                return metadataCache[entityName];

            var entityFieldMetaData = new Dictionary<string, FieldMetadata>();

            if (entityMetadataRepository == null)
                return entityFieldMetaData;

            try
            {
                var response = entityMetadataRepository.GetEntityMetadata(entityName);

                if (response == null || response.EntityMetadata?.Attributes == null)
                    return entityFieldMetaData;

                foreach (var attribute in response.EntityMetadata.Attributes)
                {
                    var fieldMetadata = new FieldMetadata();
                    fieldMetadata.AttributeType = attribute.GetType();

                    if (attribute.GetType() == typeof(LookupAttributeMetadata))
                        fieldMetadata.EntityReferenceTarget = ((LookupAttributeMetadata)attribute).Targets[0];

                    if (attribute.GetType() == typeof(DateTimeAttributeMetadata))
                        fieldMetadata.IsDateOnly = ((DateTimeAttributeMetadata)attribute).DateTimeBehavior == DateTimeBehavior.DateOnly;

                    entityFieldMetaData[attribute.LogicalName] = fieldMetadata;
                }

                metadataCache[entityName] = entityFieldMetaData;
            }
            catch (Exception)
            {
                return null;
            }

            return entityFieldMetaData;
        }

        public FieldMetadata GetAttributeType(string entityLogicalName, string fieldLogicalName, EntityMetadataRepository entitymetadataRepository)
        {
            if (metadataCache.ContainsKey(entityLogicalName) && metadataCache[entityLogicalName].ContainsKey(fieldLogicalName))
            {
                return metadataCache[entityLogicalName][fieldLogicalName];
            }

            if (entitymetadataRepository == null)
                return null;

            try
            {
                var response = entitymetadataRepository.GetEntityFieldMetadata(entityLogicalName, fieldLogicalName);

                if (response == null || response.AttributeMetadata.AttributeType == null)
                    return null;


                if (!metadataCache.ContainsKey(entityLogicalName))
                {
                    metadataCache[entityLogicalName] = new Dictionary<string, FieldMetadata>();
                }

                var fieldMetadata = new FieldMetadata();
                fieldMetadata.AttributeType = response.AttributeMetadata.GetType();

                if (response.AttributeMetadata.GetType() == typeof(LookupAttributeMetadata))
                    fieldMetadata.EntityReferenceTarget = ((LookupAttributeMetadata)response.AttributeMetadata).Targets[0];

                if (response.AttributeMetadata.GetType() == typeof(DateTimeAttributeMetadata))
                    fieldMetadata.IsDateOnly = ((DateTimeAttributeMetadata)response.AttributeMetadata).DateTimeBehavior == DateTimeBehavior.DateOnly;

                metadataCache[entityLogicalName][fieldLogicalName] = fieldMetadata;

                return fieldMetadata;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void InvalidateCache()
        {
            metadataCache.Clear();
        }
    }
}
