using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Models
{
    public class DatabaseMetadata
    {
        public List<TableMetadata> Tables { get; set; } = new List<TableMetadata>();
    }
}
