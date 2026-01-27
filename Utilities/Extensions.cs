using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

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
    }
}