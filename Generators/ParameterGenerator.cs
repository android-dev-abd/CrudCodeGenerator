using CrudCodeGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Generators
{
    public static class ParameterGenerator
    {
        public static string Generate(ColumnMetadata column,string valueExpression)
        {
            string propertyName = NamingHelper.ToPascalCase(column.Name);
            string parameterName = "@" + propertyName;
            string localParameterName = "parameter" + propertyName;
            string sqlDbType = SqlToSqlDbTypeMapper.Map(column.SqlType);

            StringBuilder code = new StringBuilder();
            
            code.AppendLine($"SqlParameter {localParameterName} = " +
                $"command.Parameters.Add(\"{parameterName}\", {sqlDbType}{GetSizeExpression(column)});");


            if (RequiresPrecisionAndScale(column))
            {
                code.AppendLine($"{localParameterName}.Precision = {column.Precision};");
                code.AppendLine($"{localParameterName}.Scale = {column.Scale};");
            }
            code.AppendLine($"{localParameterName}.Value = {GetValueExpression(column, valueExpression)};");

            return code.ToString();
        }

        private static bool RequiresSize(ColumnMetadata column)
        {
            switch(column.SqlType.ToLowerInvariant())
            {
                case "varchar":
                case "nvarchar":
                case "char":
                case "nchar":
                case "varbinary":
                case "binary":
                    return true;
                default:
                    return false;
            }
        }

        private static int GetSize(ColumnMetadata column)
        {
            if (column.MaxLength == -1) return -1;
            switch(column.SqlType.ToLowerInvariant())
            {
                case "nvarchar":
                case "nchar":
                    return column.MaxLength / 2;
                default:
                    return column.MaxLength;
            }
        }

        private static string GetSizeExpression(ColumnMetadata column)
        {
            if (!RequiresSize(column)) return string.Empty;
            return ", " + GetSize(column);
        }

        private static bool RequiresPrecisionAndScale(ColumnMetadata column)
        {
            switch (column.SqlType.ToLowerInvariant())
            {
                case "decimal":
                case "numeric":
                    return true;
                default:
                    return false;
            }
        }

        private static string GetValueExpression(ColumnMetadata column, string valueExpression)
        {
            if(column.IsNullable)
            {
                return $"{valueExpression} ?? (object)DBNull.Value";
            }
            return valueExpression;
        }

    }
}
