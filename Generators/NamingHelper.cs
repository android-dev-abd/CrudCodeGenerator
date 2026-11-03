using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Generators
{
    public static class NamingHelper
    {
        private static readonly HashSet<string> CSharpKeywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "abstract", "as", "await","base", "bool", "break", "byte", "case", "catch",
            "char", "checked", "class", "const", "continue", "decimal", "default",
            "delegate", "do", "double", "else", "enum", "event", "explicit",
            "extern", "false", "finally", "fixed", "float", "for", "foreach",
            "goto", "if", "implicit", "in", "int", "interface", "internal",
            "is", "lock", "long", "namespace", "new", "null", "object",
            "operator", "out", "override", "params", "private", "protected",
            "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string","struct", "switch",
            "this", "throw", "true", "try", "typeof","uint", "ulong", "unchecked",
            "unsafe", "ushort", "using","virtual", "void", "volatile", "while","yield"
            // Add more keywords as needed
        };

        public static string ToPascalCase(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return name;

            string[] parts = name.Split(new[] { '_', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder result = new StringBuilder();
            foreach (string part in parts)
            {
                if (part.Length == 1)
                    result.Append(part.ToUpper());
                else
                {
                    result.Append(char.ToUpper(part[0]));
                    result.Append(part.Substring(1).ToLower());
                }
            }
            return EscapeKeyword(result.ToString());
        }

        private static string EscapeKeyword(string name)
        {
            if (CSharpKeywords.Contains(name.ToLower()))
            {
                return "@" + name.ToLower();
            }
            return name;
        }

        public static string ToSingular(string name)
        {
            if(string.IsNullOrWhiteSpace(name)) return name;

            if (name.EndsWith("ies", StringComparison.OrdinalIgnoreCase))
                return name.Substring(0, name.Length - 3) + "y";

            if(
                name.EndsWith("ses",StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("xes", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("zes", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("ches", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("shes", StringComparison.OrdinalIgnoreCase)
              )
            {
                return name.Substring(0, name.Length - 2);
            }

            if (name.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                return name.Substring(0, name.Length - 1);

            return name;
        }

        public static string ToEntityName(string tableName)
        {
            string singular = ToSingular(tableName);
            return ToPascalCase(singular);
        }

        public static string ToPlural(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return name;

            if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase))
                return name.Substring(0, name.Length - 1) + "ies";

            if(name.EndsWith("s",StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("x", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("z", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("sh", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("ch", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("es", StringComparison.OrdinalIgnoreCase))
            {
                return name + "es";
            }

            return name + "s";
        }

        public static string RemoveIdSuffix(string name)
        {
            if(string.IsNullOrWhiteSpace(name)) return name;
            if(name.EndsWith("id",StringComparison.OrdinalIgnoreCase))
            {
                return name.Substring(0, name.Length - 2);
            }
            return name;
        }

        public static string EscapeCSharpStringLiteral(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var result = new System.Text.StringBuilder();

            foreach (char c in value)
            {
                switch (c)
                {
                    case '\\':
                        result.Append("\\\\");
                        break;

                    case '"':
                        result.Append("\\\"");
                        break;

                    case '\0':
                        result.Append("\\0");
                        break;

                    case '\a':
                        result.Append("\\a");
                        break;

                    case '\b':
                        result.Append("\\b");
                        break;

                    case '\f':
                        result.Append("\\f");
                        break;

                    case '\n':
                        result.Append("\\n");
                        break;

                    case '\r':
                        result.Append("\\r");
                        break;

                    case '\t':
                        result.Append("\\t");
                        break;

                    case '\v':
                        result.Append("\\v");
                        break;

                    default:
                        if (char.IsControl(c))
                        {
                            result.Append("\\u");
                            result.Append(((int)c).ToString("X4"));
                        }
                        else
                        {
                            result.Append(c);
                        }

                        break;
                }
            }

            return result.ToString();
        }

    }
}
