using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Generated
{
    public class InsertOptions
    {
        public HashSet<string> UseDefaultValueFor { get; private set; }

        public InsertOptions()
        {
            UseDefaultValueFor = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void UseDefaultValue(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("Column name cannot be empty.", nameof(columnName));
            }

            UseDefaultValueFor.Add(columnName);
        }
    }
}
