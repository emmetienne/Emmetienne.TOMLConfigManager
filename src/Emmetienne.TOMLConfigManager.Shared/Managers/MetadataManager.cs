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

        public FieldMetadata GetAttributeTypeCode(string entityLogicalName, string fieldLogicalName, EntityMetadataRepository entitymetadataRepository)
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
