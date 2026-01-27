using Emmetienne.TOMLConfigManager.Registries;

namespace Emmetienne.TOMLConfigManager.Models
{
    public class OperationExecutionContext
    {
        public RepositoryRegistry Repositories { get; }
        public TOMLOperationExecutable OperationExecutable { get; }


        public OperationExecutionContext(TOMLOperationExecutable operation, RepositoryRegistry repositories)
        {
            OperationExecutable = operation;
            Repositories = repositories;
        }
    }
}
