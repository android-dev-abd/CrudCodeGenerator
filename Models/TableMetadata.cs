using CrudCodeGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Models
{
    public class TableMetadata
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public List<ColumnMetadata> Columns { get; set; } = new List<ColumnMetadata>();
        public List<ForeignKeyMetadata> ForeignKeys { get; set; } = new List<ForeignKeyMetadata>();
        public List<UniqueKeyMetadata> UniqueKeys { get; set; } = new List<UniqueKeyMetadata>();
    }
}
