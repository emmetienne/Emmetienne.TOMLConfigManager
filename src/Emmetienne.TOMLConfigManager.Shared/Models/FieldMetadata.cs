using System;

namespace Emmetienne.TOMLConfigManager.Models
{
    public class FieldMetadata
    {
        public Type AttributeType { get; set; }
        public string EntityReferenceTarget { get; set; }
        public bool IsDateOnly { get; set; }
    }
}
