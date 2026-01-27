namespace Emmetienne.TOMLConfigManager.Services.Strategies
{
    public static class OperationStrategyFactory
    {
        public static IOperationStrategy GetStrategy(string operationType)
        {
            switch (operationType.ToLower())
            {
                case "upsert":
                    return new UpsertOperationStrategy();
                case "replace":
                    return new ReplaceOperationStrategy();
                case "delete":
                    return new DeleteOperationStrategy();
                default:
                    return null;
            }
        }
    }
}
