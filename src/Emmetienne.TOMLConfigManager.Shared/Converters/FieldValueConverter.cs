using Emmetienne.TOMLConfigManager.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Globalization;

namespace Emmetienne.TOMLConfigManager.Converters
{
    public static class FieldValueConverter
    {
        public static object Convert(string value, FieldMetadata metadata)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var type = metadata.AttributeType;

            // OptionSet
            if (type == typeof(PicklistAttributeMetadata) || type == typeof(StateAttributeMetadata) || type == typeof(StatusAttributeMetadata))
            {
                if (!int.TryParse(value, out var parsed))
                    throw new Exception($"Value '{value}' is not a valid int for OptionSet");

                return new OptionSetValue(parsed);
            }

            if (type == typeof(MultiSelectPicklistAttributeMetadata))
            {
                var values = value.Split(',');
                var optionSetValues = new OptionSetValueCollection();
                foreach (var singleValue in values)
                {
                    if (!int.TryParse(singleValue.Trim(), out var parsed))
                        throw new Exception($"Value '{singleValue}' is not a valid int for MultiSelect OptionSet");

                    optionSetValues.Add(new OptionSetValue(parsed));
                }
                return optionSetValues;
            }

            // Lookup
            if (type == typeof(LookupAttributeMetadata))
            {
                if (!Guid.TryParse(value, out var guid))
                    throw new Exception($"Value '{value}' is not a valid GUID for lookup");

                return new EntityReference(metadata.EntityReferenceTarget, guid);
            }

            // Integer
            if (type == typeof(IntegerAttributeMetadata) || type == typeof(BigIntAttributeMetadata))
            {
                if (!int.TryParse(value, out var parsed))
                    throw new Exception($"Value '{value}' is not a valid integer");

                return parsed;
            }

            // Decimal / Double / Money
            if (type == typeof(DecimalAttributeMetadata) || type == typeof(DoubleAttributeMetadata) || type == typeof(MoneyAttributeMetadata))
            {
                if (!double.TryParse(value, out var parsed))
                    throw new Exception($"Value '{value}' is not a valid number");

                return parsed;
            }

            // GUID
            if (type == typeof(UniqueIdentifierAttributeMetadata))
            {
                if (!Guid.TryParse(value, out var guid))
                    throw new Exception($"Value '{value}' is not a valid GUID");

                return guid;
            }

            // Boolean
            if (type == typeof(BooleanAttributeMetadata))
            {
                if (!bool.TryParse(value, out var parsed))
                    throw new Exception($"Value '{value}' is not a valid bool");

                return parsed;
            }

            // DateTime
            if (type == typeof(DateTimeAttributeMetadata))
            {
                var parsedDate = new DateTime();

                if (metadata.IsDateOnly && !DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out  parsedDate))
                    throw new Exception($"Value '{value}' is not a valid Date, use <yyyy-MM-dd> format ");

                string[] dateTimeFormats = { "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:sszzz" };

                if (!metadata.IsDateOnly && !DateTime.TryParseExact(value, dateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDate))
                    throw new Exception($"Value '{value}' is not a valid DateTime, use <{string.Join(",",dateTimeFormats)}> format ");

                return parsedDate;
            }

            // Default: string or memo
            return value;
        }
    }
}
