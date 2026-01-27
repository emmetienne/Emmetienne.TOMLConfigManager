using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;

namespace Emmetienne.TOMLConfigManager.Managers
{
    public class MetadataManager : Singleton<MetadataManager>
    {
        private Dictionary<string, Dictionary<string, AttributeTypeCode>> metadataCache = new Dictionary<string, Dictionary<string, AttributeTypeCode>>();

        public AttributeTypeCode? GetAttributeTypeCode(string entityLogicalName, string fieldLogicalName, EntityMetadataRepository entitymetadataRepository)
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
                    metadataCache[entityLogicalName] = new Dictionary<string, AttributeTypeCode>();
                }
                metadataCache[entityLogicalName][fieldLogicalName] = response.AttributeMetadata.AttributeType.Value;

                return response.AttributeMetadata.AttributeType.Value;
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
