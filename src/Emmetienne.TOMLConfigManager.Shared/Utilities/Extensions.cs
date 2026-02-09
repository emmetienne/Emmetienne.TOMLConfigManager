using Emmetienne.TOMLConfigManager.Models;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using Tomlyn;

namespace Emmetienne.TOMLConfigManager.Utilities
{
    public static class Extensions
    {
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
    }
}