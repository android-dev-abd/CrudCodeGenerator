using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Models
{
    public class RelationshipMetadata
    {
        public string ForeignKeyName { get; set; }
        public string SourceSchema { get; set; }
        public string SourceTable { get; set; }
        public string TargetSchema { get; set; }
        public string TargetTable { get; set; }
        public List<ForeignKeyColumnMetadata> Columns { get; set; } = new List<ForeignKeyColumnMetadata>();
        public RelationshipCardinality Cardinality { get; set; }
        public RelationshipOptionality Optionality { get; set; }

        public bool IsSelfReference
        {
            get
            {
                return SourceSchema == TargetSchema && SourceTable == TargetTable;
            }
        }
    }
}
