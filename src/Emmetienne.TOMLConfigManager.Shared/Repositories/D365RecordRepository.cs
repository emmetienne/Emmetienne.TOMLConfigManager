using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace Emmetienne.TOMLConfigManager.Repositories
{
    public class D365RecordRepository
    {
        private readonly IOrganizationService organizationService;

        public D365RecordRepository(IOrganizationService organizationService)
        {
            this.organizationService = organizationService;
        }

        public EntityCollection GetRecordFromEnvironment(string entityLogicalName, List<string> fieldLogicalNames, List<string> fieldValues, bool retrieveAllFields)
        {
            if (fieldLogicalNames == null)
                throw new ArgumentNullException($"No match on has been provided");

            if (fieldValues == null)
                throw new ArgumentNullException($"No fields has been provided");

            if (fieldValues.Count != fieldLogicalNames.Count)
                throw new Exception("Fields and matching does not have the same count");

            var query = new QueryExpression(entityLogicalName);
            query.NoLock = true;
            query.ColumnSet.AllColumns = retrieveAllFields;

            for (int i = 0; i < fieldValues.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(fieldValues[i]))
                    query.Criteria.AddCondition(fieldLogicalNames[i], ConditionOperator.Null);
                else
                    query.Criteria.AddCondition(fieldLogicalNames[i], ConditionOperator.Equal, fieldValues[i]);
            }

            return organizationService.RetrieveMultiple(query);
        }

        public void DeleteRecord(string entityLogicalName, Guid recordId)
        {
            organizationService.Delete(entityLogicalName, recordId);
        }

        public void UpdateRecord(Entity record)
        {
            organizationService.Update(record);
        }

        public Guid CreateRecord(Entity record)
        {
            return organizationService.Create(record);
        }

        public Entity GetRecorById(string entityLogicalName, Guid recordId, string[] fieldLogicalNames, bool retrieveAllFields)
        {
            var columnSet = new ColumnSet(false);

            if (retrieveAllFields)
            {
                columnSet.AllColumns = true;
                return organizationService.Retrieve(entityLogicalName, recordId, columnSet);
            }

            if (fieldLogicalNames == null || fieldLogicalNames.Length == 0)
                throw new ArgumentException("No fields to be retrieved has been provided and retrieve all fields is false");

            columnSet.AddColumns(fieldLogicalNames);
            return organizationService.Retrieve(entityLogicalName, recordId, columnSet);
        }
    }
}