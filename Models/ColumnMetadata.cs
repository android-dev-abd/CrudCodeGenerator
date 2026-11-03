using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Models
{
    public class ColumnMetadata
    {
        public string Name { get; set; }
        public string SqlType { get; set; }
        public int MaxLength { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsComputed { get; set; }
        public bool IsPrimaryKey { get; set; }
        public int KeyOrdinal { get; set; }
        public bool HasDefaultValue { get; set; }

    }
}
