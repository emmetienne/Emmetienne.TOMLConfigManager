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

                // Ensure the inner dictionary exists before assignment
                if (!metadataCache.ContainsKey(entityLogicalName))
                {
                    metadataCache[entityLogicalName] = new Dictionary<string, FieldMetadata>();
                }

                var fieldMetadata = new FieldMetadata();
                fieldMetadata.AttributeTypeCode = response.AttributeMetadata.AttributeType.Value;

                if (response.AttributeMetadata.AttributeType == AttributeTypeCode.Lookup)
                    fieldMetadata.EntityReferenceTarget = ((LookupAttributeMetadata)response.AttributeMetadata).Targets[0];

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
