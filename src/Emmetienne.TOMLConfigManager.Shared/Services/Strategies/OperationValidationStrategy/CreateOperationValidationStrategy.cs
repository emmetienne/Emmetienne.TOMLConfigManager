using Emmetienne.TOMLConfigManager.Constants;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using System;
using System.Collections.Generic;

namespace Emmetienne.TOMLConfigManager.Services.Strategies.OperationValidationStrategy
{
    public class CreateOperationValidationStrategy : IOperationValidationStrategy
    {
        public bool ValidateOperation(OperationExecutionContext operationExecutionContext)
        {
            var errorList = new List<string>();

            var operation = operationExecutionContext.OperationExecutable;

            var targetMetadataRepository = operationExecutionContext.Repositories.Get<EntityMetadataRepository>(RepositoryRegistryKeys.targetEntityMetadataRepository);

            if (string.IsNullOrWhiteSpace(operation.Table))
                errorList.Add("Table name is required.");

            var entityMetadata = targetMetadataRepository.GetEntityMetadata(operation.Table);

            if (entityMetadata == null)
            {
                errorList.Add($"Table '{operation.Table}' does not exist.");
                return false;
            }

            if (operation.Fields == null || operation.Fields.Count == 0)
                errorList.Add("At least one field is required.");

            if (operation.Fields != null)
            {
                for (int i = 0; i < operation.Fields.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(operation.Fields[i]))
                    {
                        errorList.Add($"Field attribute in position <{i}> cannot be blank");
                        continue;
                    }
                }
            }

            if (operation.Values == null || operation.Values.Count == 0)
                errorList.Add("At least one value is required.");

            if (operation.Fields?.Count != operation.Values?.Count)
            {
                errorList.Add("The number of fields must match the number of values.");
            }

            if (errorList.Count > 0)
            {
                var errorMessage = string.Join(Environment.NewLine, errorList);
                operation.ErrorMessage = errorMessage;
            }

            return errorList.Count == 0;
        }
    }
}
