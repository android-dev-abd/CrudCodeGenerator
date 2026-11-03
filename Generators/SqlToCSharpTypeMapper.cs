using CrudCodeGenerator.Configuration;
using CrudCodeGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Generators
{
    public static class SqlToCSharpTypeMapper
    {
        public static string Map(string sqlType)
        {
            if (string.IsNullOrWhiteSpace(sqlType))
                throw new ArgumentException("SQL type cannot be null or empty.", nameof(sqlType));

            switch (sqlType.ToLowerInvariant())
            {
                case "int":
                    return "int";
                case "bigint":
                    return "long";
                case "smallint":
                    return "short";
                case "tinyint":
                    return "byte";
                case "bit":
                    return "bool";
                case "decimal":
                case "numeric":
                case "money":
                case "smallmoney":
                    return "decimal";
                case "float":
                    return "double";
                case "real":
                    return "float";
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return "DateTime";
                case "time":
                    return "TimeSpan";
                case "char":
                case "varchar":
                case "text":
                case "nchar":
                case "nvarchar":
                case "ntext":
                case "xml":
                case "sysname":
                    return "string";
                case "binary":
                case "varbinary":
                case "image":
                case "timestamp":
                case "rowversion":
                    return "byte[]";
                case "uniqueidentifier":
                    return "Guid";               
                default:
                    throw new NotSupportedException($"SQL type '{sqlType}' is not supported.");
            }

        }
    

        public static string MapColumn(ColumnMetadata column,GeneratorOptions options)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            string csharpType = Map(column.SqlType);

            if(!column.IsNullable)
                return csharpType;

            if(IsValueType(csharpType))
                return csharpType + "?";

            if(options.EnableNullableReferenceTypes)
                return csharpType + "?";

            return csharpType;
        }

        private static bool IsValueType(string csharpType)
        {            
            switch (csharpType)
            {
                case "bool":
                case "byte":
                case "sbyte":
                case "short":
                case "ushort":
                case "int":
                case "uint":
                case "long":
                case "ulong":
                case "float":
                case "double":
                case "decimal":
                case "char":
                case "DateTime":
                case "Guid":
                case "TimeSpan":
                    return true;
                default:
                    return false;
            }
        }
    }
}
