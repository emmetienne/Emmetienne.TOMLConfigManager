using System;
using System.Collections.Generic;
using Tomlyn;

namespace Emmetienne.TOMLConfigManager.Models
{
    public class TOMLOperationExecutable
    {
        public Guid OperationId { get; protected set; }
        public string Type { get; set; }
        public string Table { get; set; }
        public List<string> MatchOn { get; set; } = new List<string>();
        public List<string> Row { get; set; } = new List<string>();
        public List<string> IgnoreFields { get; set; } = new List<string>();
        public List<string> Fields { get; set; } = new List<string>();
        public List<string> Values { get; set; } = new List<string>();

        public TOMLOperationExecutable()
        {
            OperationId = Guid.NewGuid();
        }

        public string ErrorMessage { get; set; }
        public string WarningMessage { get; set; }
        public bool Success => string.IsNullOrWhiteSpace(ErrorMessage) || string.IsNullOrWhiteSpace(WarningMessage);

        public override string ToString()
        {
            return Toml.FromModel(this);
        }
    }
}