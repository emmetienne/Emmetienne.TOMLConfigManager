using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Emmetienne.TOMLConfigManager.Repositories
{
    public class EntityMetadataRepository
    {
        private IOrganizationService service;

        public EntityMetadataRepository(IOrganizationService service)
        {
            this.service = service;
        }

        public RetrieveAttributeResponse GetEntityFieldMetadata(string entityLogicalName, string fieldLogicalName)
        {
            var retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = fieldLogicalName,
                RetrieveAsIfPublished = true
            };

            return (RetrieveAttributeResponse)service.Execute(retrieveAttributeRequest);
        }

        public RetrieveEntityResponse GetEntityMetadata(string entityLogicalName)
        {
            var retrieveEntityRequest = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = entityLogicalName,
                RetrieveAsIfPublished = true
            };

            return (RetrieveEntityResponse)service.Execute(retrieveEntityRequest);
        }
    }
}
