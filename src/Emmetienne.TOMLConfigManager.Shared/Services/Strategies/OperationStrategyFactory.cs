using Emmetienne.TOMLConfigManager.Logger;

namespace Emmetienne.TOMLConfigManager.Services.Strategies
{
    public static class OperationStrategyFactory
    {
        public static IOperationStrategy GetStrategy(string operationType, ILogger logger)
        {
            switch (operationType.ToLower())
            {
                case "upsert":
                    return new UpsertOperationStrategy(logger);
                case "replace":
                    return new ReplaceOperationStrategy(logger);
                case "delete":
                    return new DeleteOperationStrategy(logger);
                case "create":
                    return new CreateOperationStrategy(logger);
                default:
                    return null;
            }
        }
    }
}
