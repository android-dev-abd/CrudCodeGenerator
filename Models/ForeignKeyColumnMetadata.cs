using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Models
{
    public class ForeignKeyColumnMetadata
    {
        public string ParentColumn { get; set; }
        public string ReferencedColumn { get; set; }
    }
}
