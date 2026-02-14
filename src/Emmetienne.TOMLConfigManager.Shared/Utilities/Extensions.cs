using Emmetienne.TOMLConfigManager.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using Tomlyn;

namespace Emmetienne.TOMLConfigManager.Utilities
{
    public static class Extensions
    {
        public static Entity DeepClone(this Entity entity)
        {
            if (entity == null)
                return null;

            var clone = new Entity(entity.LogicalName)
            {
                Id = entity.Id
            };

            foreach (var attribute in entity.Attributes)
            {
                clone[attribute.Key] = CloneAttributeValue(attribute.Value);
            }

            return clone;
        }

        private static object CloneAttributeValue(object value)
        {
            if (value == null)
                return null;

            switch (value)
            {
                case EntityReference entityRef:
                    return new EntityReference(entityRef.LogicalName, entityRef.Id) { Name = entityRef.Name };

                case OptionSetValue optionSet:
                    return new OptionSetValue(optionSet.Value);

                case Money money:
                    return new Money(money.Value);

                case AliasedValue aliased:
                    return new AliasedValue(aliased.EntityLogicalName, aliased.AttributeLogicalName, CloneAttributeValue(aliased.Value));

                case EntityCollection entityCollection:
                    return new EntityCollection(entityCollection.Entities.Select(e => e.DeepClone()).ToList());

                case OptionSetValueCollection optionSetCollection:
                    return new OptionSetValueCollection(optionSetCollection.Select(o => new OptionSetValue(o.Value)).ToList());

                case byte[] bytes:
                    var bytesCopy = new byte[bytes.Length];
                    Array.Copy(bytes, bytesCopy, bytes.Length);
                    return bytesCopy;

                case string _:
                case int _:
                case long _:
                case decimal _:
                case double _:
                case float _:
                case bool _:
                case DateTime _:
                case Guid _:
                    return value;

                default:
                    return value;
            }
        }

        public static Entity RemoveOutOfTheBoxAndExcludedFields(this Entity record, List<string> excludeList)
        {
            var outOfTheBoxFields = new List<string>
            {
                "createdby",
                "createdon",
                "createdonbehalfby",
                "modifiedby",
                "modifiedon",
                "modifiedonbehalfby",
                "ownerid",
                "owningbusinessunit",
                "owninguser",
                "versionnumber"
            };

            foreach (var field in outOfTheBoxFields)
            {
                record.Attributes.Remove(field);
            }

            if (excludeList == null || excludeList.Count == 0)
                return record;

            foreach (var field in excludeList)
            {
                record.Attributes.Remove(field);
            }

            return record;
        }

        public static TOMLOperationExecutable ToTOMLOperationExecutable(this TOMLOperationRaw raw, int rowIndex)
        {
            var executable = new TOMLOperationExecutable();

            var supportedTypes = new HashSet<string> { Constants.OperationTypes.create, Constants.OperationTypes.replace, Constants.OperationTypes.delete, Constants.OperationTypes.upsert };

            if (!supportedTypes.Contains(raw.Type))
                throw new Exception($"No operation type of '{raw.Type}' can be parsed for the following TOML operation{Environment.NewLine}{Toml.FromModel(raw)}");

            executable.Type = raw.Type ?? string.Empty;
            executable.Table = raw?.Table ?? string.Empty;
            executable.MatchOn = raw?.MatchOn ?? new List<string>();
            executable.IgnoreFields = raw?.IgnoreFields ?? new List<string>();
            executable.Fields = raw?.Fields ?? new List<string>();
            executable.Values = raw?.Values ?? new List<string>();
            executable.Row = raw.Rows != null && raw.Rows.Count > 0 ? raw.Rows[rowIndex] : new List<string>();

            return executable;
        }
        public static bool IsValidBase64(this string base64String)
        {
            if (string.IsNullOrWhiteSpace(base64String))
                return false;

            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool IsFileOrImageField(this Type attributeMetadataType)
        {
            return attributeMetadataType == typeof(FileAttributeMetadata) || attributeMetadataType == typeof(ImageAttributeMetadata);
        }
    }
}