using CrudCodeGenerator.Configuration;
using CrudCodeGenerator.Models;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Generators
{
    public static class ReaderMapper
    {
        public static string GenerateAssignment(ColumnMetadata column,string propertyName,GeneratorOptions options,int columnIndex)
        {
            string escapedColumnName = NamingHelper.EscapeCSharpStringLiteral(column.Name);

            string ordinalVariable = "ordinal" + columnIndex;

            string indexExpression = $"reader.GetOrdinal(\"{escapedColumnName}\")";

            string readMethod = GetReadMethod(column.SqlType);

            string readExpression;

            if (readMethod == "GetValue")
                readExpression = $"(byte[])reader.GetValue({ordinalVariable})";
            else
                readExpression = $"reader.{readMethod}({ordinalVariable})";

            string assignmentExpression;

            if (column.IsNullable)
            {
                string csharpType = SqlToCSharpTypeMapper.MapColumn(column, options);

                assignmentExpression = $"reader.IsDBNull({ordinalVariable}) ? default({csharpType}) : {readExpression}";
            }
            else
            {
                assignmentExpression = readExpression;
            }

            return
                $"int {ordinalVariable} = {indexExpression};" +
                Environment.NewLine +
                $"entity.{propertyName} = {assignmentExpression};";
        }

        private static string GetReadMethod(string sqlType)
        {
            switch (sqlType.ToLowerInvariant())
            {
                case "bigint":
                    return "GetInt64";

                case "int":
                    return "GetInt32";

                case "smallint":
                    return "GetInt16";

                case "tinyint":
                    return "GetByte";

                case "bit":
                    return "GetBoolean";

                case "decimal":
                case "numeric":
                case "money":
                case "smallmoney":
                    return "GetDecimal";

                case "float":
                    return "GetDouble";

                case "real":
                    return "GetFloat";

                case "char":
                case "varchar":
                case "text":
                case "nchar":
                case "nvarchar":
                case "ntext":
                case "xml":
                case "sysname":
                    return "GetString";

                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return "GetDateTime";

                case "time":
                    return "GetTimeSpan";

                case "uniqueidentifier":
                    return "GetGuid";

                case "binary":
                case "varbinary":
                case "image":
                case "timestamp":
                case "rowversion":
                    return "GetValue";

                default:
                    throw new NotSupportedException(
                        $"SQL type '{sqlType}' is not supported.");
            }
        }

        public static T Map<T>(SqlDataReader reader) where T: new()
        {
            T entity = new T();
            Type entityType = typeof(T);

            foreach(PropertyInfo property in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanWrite) continue;

                string columnName = property.Name;
                int ordinal;
                try
                {
                    ordinal = reader.GetOrdinal(columnName);
                }
                catch(IndexOutOfRangeException)
                {
                    continue;
                }

                if(reader.IsDBNull(ordinal))
                {
                    if(!property.PropertyType.IsValueType || Nullable.GetUnderlyingType(property.PropertyType) != null)
                    property.SetValue(entity,null, null);
                    continue;
                }

                object value = reader.GetValue(ordinal);
                object convertedValue = ConvertValue(value, property.PropertyType);
                property.SetValue(entity,convertedValue,null);
            }
            return entity;
        }

        private static object ConvertValue(object value, Type targetType)
        {
            if(value == null) return null;

            Type actualType = Nullable.GetUnderlyingType(targetType);
            if (actualType == null) actualType = targetType;

            if (actualType.IsEnum) return Enum.ToObject(actualType, value);

            if(actualType == typeof(Guid))
            {
                if (value is Guid) return value;

                return Guid.Parse(value.ToString());
            }

            if (actualType == typeof(byte[])) return (byte[])value;

            if (actualType.IsAssignableFrom(value.GetType())) return value;

            return Convert.ChangeType(value, actualType);
        }

    }
}
