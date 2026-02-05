using Emmetienne.TOMLConfigManager.Constants;
using Emmetienne.TOMLConfigManager.Logger;

namespace Emmetienne.TOMLConfigManager.Services.Strategies
{
    public static class OperationStrategyFactory
    {
        public static IOperationStrategy GetStrategy(string operationType, ILogger logger)
        {
            switch (operationType.ToLower())
            {
                case OperationTypes.upsert:
                    return new UpsertOperationStrategy(logger);
                case OperationTypes.replace:
                    return new ReplaceOperationStrategy(logger);
                case OperationTypes.delete:
                    return new DeleteOperationStrategy(logger);
                case OperationTypes.create:
                    return new CreateOperationStrategy(logger);
                default:
                    return null;
            }
        }
    }
}
