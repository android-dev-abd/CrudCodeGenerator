using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Models
{
    public class UniqueKeyMetadata
    {
        public string Name { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public bool IsUniqueConstraint { get; set; }
        public List<string> Columns { get; set; } = new List<string>();
    }
}
