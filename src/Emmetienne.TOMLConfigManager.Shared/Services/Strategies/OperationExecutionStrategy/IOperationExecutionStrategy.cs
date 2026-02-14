using Emmetienne.TOMLConfigManager.Models;

namespace Emmetienne.TOMLConfigManager.Services.Strategies.OperationExecutionStrategy
{
    public interface IOperationExecutionStrategy
    {
        void ExecuteOperation(OperationExecutionContext operationExecutionContext);
    }
}