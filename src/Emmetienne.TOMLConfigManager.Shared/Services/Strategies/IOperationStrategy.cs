using Emmetienne.TOMLConfigManager.Models;

namespace Emmetienne.TOMLConfigManager.Services.Strategies
{
    public interface IOperationStrategy
    {
        void ExecuteOperation(OperationExecutionContext operationExecutionContext);
    }
}