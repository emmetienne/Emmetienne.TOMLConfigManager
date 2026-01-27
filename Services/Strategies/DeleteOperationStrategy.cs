using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;

namespace Emmetienne.TOMLConfigManager.Services.Strategies
{
    internal class DeleteOperationStrategy : IOperationStrategy
    {
        public void ExecuteOperation(OperationExecutionContext operationExecutionContext)
        {
            var d365RecordRepository = operationExecutionContext.Repositories.Get<D365RecordRepository>("Target.RecordRepository");
            var operation = operationExecutionContext.OperationExecutable;

            var record = d365RecordRepository.GetRecordFromEnvironment(operation.Table, operation.MatchOn, operation.Row, false);

            if (record.Entities.Count == 0)
            {
                operation.ErrorMessage = $"No record on target environment match the criteria.";
                return;
            }

            d365RecordRepository.DeleteRecord(record.Entities[0].LogicalName, record.Entities[0].Id);
        }
    }
}