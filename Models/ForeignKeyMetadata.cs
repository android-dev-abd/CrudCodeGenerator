using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Models
{
    public class ForeignKeyMetadata
    {
        public string Name { get; set; }
        public string ParentSchema { get; set; }
        public string ParentTable { get; set; }
        public string ReferencedSchema { get; set; }
        public string ReferencedTable { get; set; }
        public List<ForeignKeyColumnMetadata> Columns { get; set; } = new List<ForeignKeyColumnMetadata>();
    }
}
