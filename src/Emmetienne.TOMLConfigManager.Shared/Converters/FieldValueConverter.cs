using Emmetienne.TOMLConfigManager.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;

namespace Emmetienne.TOMLConfigManager.Converters
{
    public static class FieldValueConverter
    {
        public static object Convert(string value,FieldMetadata metadata)
        {
            var type = metadata.AttributeTypeCode;

            // OptionSet
            if (type == AttributeTypeCode.Picklist || type == AttributeTypeCode.State || type == AttributeTypeCode.Status)
            {
                if (!int.TryParse(value, out var parsed))
                    throw new Exception($"Value '{value}' is not a valid int for OptionSet");

                return new OptionSetValue(parsed);
            }

            // Lookup
            if (type == AttributeTypeCode.Lookup)
            {
                if (!Guid.TryParse(value, out var guid))
                    throw new Exception($"Value '{value}' is not a valid GUID for lookup");

                   return new EntityReference(metadata.EntityReferenceTarget, guid);
            }

            // Integer
            if (type == AttributeTypeCode.Integer || type == AttributeTypeCode.BigInt)
            {
                if (!int.TryParse(value, out var parsed))
                    throw new Exception($"Value '{value}' is not a valid integer");

                return parsed;
            }

            // GUID
            if (type == AttributeTypeCode.Uniqueidentifier)
            {
                if (!Guid.TryParse(value, out var guid))
                    throw new Exception($"Value '{value}' is not a valid GUID");

                return guid;
            }

            // Decimal / Double / Money
            if (type == AttributeTypeCode.Decimal || type == AttributeTypeCode.Double || type == AttributeTypeCode.Money)
            {
                if (!double.TryParse(value, out var parsed))
                    throw new Exception($"Value '{value}' is not a valid number");

                return parsed;
            }

            // Boolean
            if (type == AttributeTypeCode.Boolean)
            {
                if (!bool.TryParse(value, out var parsed))
                    throw new Exception($"Value '{value}' is not a valid bool");

                return parsed;
            }

            // Default: string
            return value;
        }
    }
}
