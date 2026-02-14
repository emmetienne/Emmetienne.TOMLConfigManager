using Emmetienne.TOMLConfigManager.Models;

namespace Emmetienne.TOMLConfigManager.Services.Strategies.OperationValidationStrategy
{
    public interface IOperationValidationStrategy
    {
        bool ValidateOperation(OperationExecutionContext operationExecutionContext);
    }
}