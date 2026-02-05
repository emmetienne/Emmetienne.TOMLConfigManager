using Emmetienne.TOMLConfigManager.Eventbus;
using Emmetienne.TOMLConfigManager.Models;
using System;
using System.Collections.Generic;

namespace Emmetienne.TOMLConfigManager.Stores
{
    public class OperationsStoreSingleton : Singleton<OperationsStoreSingleton>
    {
        private Dictionary<Guid, TOMLOperationExecutable> TOMLOperationsExecutable = new Dictionary<Guid, TOMLOperationExecutable>();

        public void InitializeOperationStore()
        {
            this.TOMLOperationsExecutable = new Dictionary<Guid, TOMLOperationExecutable>();
        }

        public void AddOperationExecutable(TOMLOperationExecutable operationExecutable)
        {
            TOMLOperationsExecutable[operationExecutable.OperationId] = operationExecutable;
        }

        public TOMLOperationExecutable GetOperationExecutable(Guid operationId)
        {
            if (TOMLOperationsExecutable.TryGetValue(operationId, out var operationExecutable))
            {
                return operationExecutable;
            }

            return null;
        }

        public List<TOMLOperationExecutable> GetAllOperationExecutables()
        {
            return new List<TOMLOperationExecutable>(TOMLOperationsExecutable.Values);
        }
    }
}
