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

        // testare se è tutto fattibile oppure ci sono casi particolari di cast non automatici (tipo int o simili, o magari se boxando funziona)
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
                query.Criteria.AddCondition(fieldLogicalNames[i], ConditionOperator.Equal, fieldValues[i]);

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
    }
}