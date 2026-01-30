using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Utilities;
using System;
using System.Collections.Generic;
using Tomlyn;

namespace Emmetienne.TOMLConfigManager.Services
{
    public class TOMLParsingService
    {
        public List<TOMLOperationExecutable> ParseToTOMLExecutables(string TOMLcontent)
        {
            var TOMLOperationsDeserialized = Toml.ToModel<TOMLParsed>(TOMLcontent);

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
                        throw new Exception(
                           $"The numbers of fields in the row do not match the expected count for the followin TOML:{Environment.NewLine}{Toml.FromModel(singleTOMLOperation)}"
                        );
                    }

                    if (singleTOMLOperation.Fields?.Count != singleTOMLOperation.Values?.Count)
                    {
                        throw new Exception(
                           $"The numbers of fields and values do not match for the following TOML:{Environment.NewLine}{Toml.FromModel(singleTOMLOperation)}"
                        );
                    }

                    TOMLExecutables.Add(singleTOMLOperation.ToTOMLOperationExecutable(i));
                }
            }

            return TOMLExecutables;
        }
    }
}