using Microsoft.Xrm.Sdk.Metadata;

namespace Emmetienne.TOMLConfigManager.Models
{
    public class FieldMetadata
    {
        public AttributeTypeCode? AttributeTypeCode { get; set; }
        public string EntityReferenceTarget { get; set; }
    }
}
