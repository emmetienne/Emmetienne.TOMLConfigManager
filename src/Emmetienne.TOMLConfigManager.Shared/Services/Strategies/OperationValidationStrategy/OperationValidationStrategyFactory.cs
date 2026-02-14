using Emmetienne.TOMLConfigManager.Constants;

namespace Emmetienne.TOMLConfigManager.Services.Strategies.OperationValidationStrategy
{
    public static class OperationValidationStrategyFactory
    {
        public static IOperationValidationStrategy GetStrategy(string operationType)
        {
            switch (operationType.ToLower())
            {
                case OperationTypes.replace:
                    return new ReplaceOperationValidationStrategy();
                case OperationTypes.upsert:
                case OperationTypes.delete:
                    return new UpsertDeleteOperationValidationStrategy();
                case OperationTypes.create:
                    return new CreateOperationValidationStrategy();
                default:
                    return null;
            }
        }
    }
}
