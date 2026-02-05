using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Utilities;
using System;
using System.Collections.Generic;
using Tomlyn;
using Tomlyn.Syntax;

namespace Emmetienne.TOMLConfigManager.Services
{
    public class TOMLParsingService
    {
        public List<TOMLOperationExecutable> ParseToTOMLExecutables(string TOMLcontent, ILogger logger)
        {
            var TOMLOperationsDeserialized = new TOMLParsed();

            try
            {
                TOMLOperationsDeserialized = Toml.ToModel<TOMLParsed>(TOMLcontent);
                //if (!Toml.TryToModel<TOMLParsed>(TOMLcontent, out TOMLOperationsDeserialized, out DiagnosticsBag diagnostics))
                //{
                //    var errorMessage = "Failed to deserialize the provided TOML content, the provided TOML is not formally valid";
                //    logger.LogError(errorMessage);
                //    return null;
                //}
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to deserialize the provided TOML content. Error: {ex.Message}";
                logger.LogError(errorMessage);
                return null;
            }

            var TOMLExecutables = new List<TOMLOperationExecutable>();

            foreach (var singleTOMLOperation in TOMLOperationsDeserialized.operation)
            {
                if (singleTOMLOperation.Rows == null || singleTOMLOperation.Rows.Count == 0)
                {
                    TOMLExecutables.Add(singleTOMLOperation.ToTOMLOperationExecutable(0));
                    continue;
                }

                for (int i = 0; i < singleTOMLOperation.Rows.Count; i++)
                {
                    var row = singleTOMLOperation.Rows[i];
                    int expected = singleTOMLOperation.MatchOn.Count;

                    if (row.Count != expected)
                    {
                        var errorMessage = $"The numbers of fields in the row do not match the expected count for the following TOML:{Environment.NewLine}{Toml.FromModel(singleTOMLOperation)}";

                        logger.LogError(errorMessage);
                        throw new Exception(errorMessage);
                    }

                    if (singleTOMLOperation.Fields?.Count != singleTOMLOperation.Values?.Count)
                    {
                        var errorMessage = $"The numbers of fields and values do not match for the following TOML:{Environment.NewLine}{Toml.FromModel(singleTOMLOperation)}";
                        logger.LogError(errorMessage);
                        throw new Exception(errorMessage);
                    }

                    TOMLExecutables.Add(singleTOMLOperation.ToTOMLOperationExecutable(i));
                }
            }

            return TOMLExecutables;
        }
    }
}