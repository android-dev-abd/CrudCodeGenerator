using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Generators
{
    public static class SqlToSqlDbTypeMapper
    {
        public static string Map(string sqlType)
        {
            switch (sqlType.ToLowerInvariant())
            {
                case "int":
                    return "SqlDbType.Int";
                case "bigint":
                    return "SqlDbType.BigInt";
                case "smallint":
                    return "SqlDbType.SmallInt";
                case "tinyint":
                    return "SqlDbType.TinyInt";
                case "bit":
                    return "SqlDbType.Bit";
                case "decimal":
                case "numeric":
                    return "SqlDbType.Decimal";
                case "money":
                    return "SqlDbType.Money";
                case "smallmoney":
                    return "SqlDbType.SmallMoney";
                case "float":
                    return "SqlDbType.Float";
                case "real":
                    return "SqlDbType.Real";
                case "date":
                    return "SqlDbType.Date";
                case "datetime":
                    return "SqlDbType.DateTime";
                case "datetime2":
                    return "SqlDbType.DateTime2";
                case "smalldatetime":
                    return "SqlDbType.SmallDateTime";
                case "time":
                    return "SqlDbType.Time";
                case "char":
                    return "SqlDbType.Char";
                case "varchar":
                    return "SqlDbType.VarChar";
                case "text":
                    return "SqlDbType.Text";
                case "nchar":
                    return "SqlDbType.NChar";
                case "nvarchar":
                    return "SqlDbType.NVarChar";
                case "ntext":
                    return "SqlDbType.NText";
                case "binary":
                    return "SqlDbType.Binary";
                case "varbinary":
                    return "SqlDbType.VarBinary";
                case "image":
                    return "SqlDbType.Image";
                case "uniqueidentifier":
                    return "SqlDbType.UniqueIdentifier";
                case "xml":
                    return "SqlDbType.Xml";
                default:
                    throw new NotSupportedException($"SQL type '{sqlType}' is not supported.");
            }
        }
    }
}