using Emmetienne.TOMLConfigManager.Constants;
using Emmetienne.TOMLConfigManager.Logger;

namespace Emmetienne.TOMLConfigManager.Services.Strategies.OperationExecutionStrategy
{
    public static class OperationExecutionStrategyFactory
    {
        public static IOperationExecutionStrategy GetStrategy(string operationType, ILogger logger)
        {
            switch (operationType.ToLower())
            {
                case OperationTypes.upsert:
                    return new UpsertOperationExecutionStrategy(logger);
                case OperationTypes.replace:
                    return new ReplaceOperationExecutionStrategy(logger);
                case OperationTypes.delete:
                    return new DeleteOperationExecutionStrategy(logger);
                case OperationTypes.create:
                    return new CreateOperationExecutionStrategy(logger);
                default:
                    return null;
            }
        }
    }
}
