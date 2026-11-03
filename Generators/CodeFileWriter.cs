using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Generators
{
    public static class CodeFileWriter
    {
        public static void Write(string outputDirectory, string fileName, string code)
        {
            if(string.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new ArgumentException("Outputdirectorey can not be empty.", nameof(outputDirectory));
            }

            if(string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name can not be empty.", nameof(fileName));
            }

            if(code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            string filPath = Path.Combine(outputDirectory, fileName);

            File.WriteAllText(filPath, code,new UTF8Encoding(false));
        }
    }
}
