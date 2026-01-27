using System;
using System.Collections.Generic;

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
        public List<string> Fields { get; set; }
        public List<string> Values { get; set; }

        public TOMLOperationExecutable()
        {
            OperationId = Guid.NewGuid();
        }

        public string ErrorMessage { get; set; }
        public bool Success => string.IsNullOrWhiteSpace(ErrorMessage);
    }
}
