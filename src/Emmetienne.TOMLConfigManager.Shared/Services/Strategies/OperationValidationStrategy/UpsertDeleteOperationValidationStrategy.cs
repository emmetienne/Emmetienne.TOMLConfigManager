using Emmetienne.TOMLConfigManager.Constants;
using Emmetienne.TOMLConfigManager.Managers;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Emmetienne.TOMLConfigManager.Utilities;
using System;
using System.Collections.Generic;

namespace Emmetienne.TOMLConfigManager.Services.Strategies.OperationValidationStrategy
{
    public class UpsertDeleteOperationValidationStrategy : IOperationValidationStrategy
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

            if (operation.MatchOn == null || operation.MatchOn.Count == 0)
                errorList.Add("At least one match-on field is required.");

            if (operation.MatchOn != null)
            {
                for (int i = 0; i < operation.MatchOn.Count; i++)
                {
                    {
                        var matchField = operation.MatchOn[i];

                        if (string.IsNullOrWhiteSpace(matchField))
                        {
                            errorList.Add($"Match-on field in position <{i}> cannot be blank");
                            continue;
                        }

                        var fieldMetadata = MetadataManager.Instance.GetAttributeType(operation.Table, matchField, targetMetadataRepository);

                        if (fieldMetadata == null)
                            errorList.Add($"Match-on field '{matchField}' does not exist in table '{operation.Table}'.");

                        if (fieldMetadata.AttributeType.IsFileOrImageField())
                            errorList.Add($"Match-on fields cannot be of file or image type. Field <{matchField}>");
                    }
                }
            }

            if (operation.Row == null || operation.Row.Count == 0)
                errorList.Add("Row data is required.");

            if (operation.Row != null)
            {
                for (int i = 0; i < operation.Row.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(operation.Row[i]))
                    {
                        errorList.Add($"Row attribute in position <{i}> cannot be blank");
                        continue;
                    }
                }
            }

            if (operation.MatchOn?.Count != operation.Row?.Count)
                errorList.Add("The number of match-on fields must match the number of row values.");

            if (errorList.Count > 0)
            {
                var errorMessage = string.Join(Environment.NewLine, errorList);
                operation.ErrorMessage = errorMessage;
            }

            return errorList.Count == 0;
        }
    }
}
