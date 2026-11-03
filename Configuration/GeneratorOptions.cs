using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Configuration
{
    public class GeneratorOptions
    {
        public string CSharpLanguageVersion { get; set; } = "7.3";
        public bool EnableNullableReferenceTypes { get; set; } = false;

        public void Validate()
        {
            if (EnableNullableReferenceTypes &&
                CSharpLanguageVersion != "8.0" &&
                CSharpLanguageVersion != "9.0" &&
                CSharpLanguageVersion != "10.0" &&
                CSharpLanguageVersion != "11.0" &&
                CSharpLanguageVersion != "12.0" &&
                CSharpLanguageVersion != "13.0")
            {
                throw new System.InvalidOperationException(
                    "Nullable Reference Types require C# 8.0 or later.");
            }
        }
    }
}
