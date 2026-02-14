using Emmetienne.TOMLConfigManager.Registries;

namespace Emmetienne.TOMLConfigManager.Models
{
    public class OperationExecutionContext
    {
        public RepositoryRegistry Repositories { get; }
        public TOMLOperationExecutable OperationExecutable { get; }
        public string FileSourceBasePath { get; } 


        public OperationExecutionContext(TOMLOperationExecutable operation, RepositoryRegistry repositories, string fileSourceBasePath)
        {
            this.OperationExecutable = operation;
            this.Repositories = repositories;
            this.FileSourceBasePath = fileSourceBasePath;
        }
    }
}
