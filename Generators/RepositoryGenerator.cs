using CrudCodeGenerator.Configuration;
using CrudCodeGenerator.Models;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Generators
{
    public static class RepositoryGenerator
    {
        public static string Generate(TableMetadata table,GeneratorOptions options)
        {
            StringBuilder code = new StringBuilder();
            string entityName = NamingHelper.ToEntityName(table.TableName);
            string repositoryName = entityName + "Repository";
           

            code.AppendLine("using System;");
            code.AppendLine("using System.Text;");
            code.AppendLine("using System.Collections.Generic;");
            code.AppendLine("using System.Linq;");
            code.AppendLine("using System.Data;");
            code.AppendLine("using System.Data.SqlClient;");
            code.AppendLine("using Generated.Entities;");
            code.AppendLine("using CrudCodeGenerator.Generators;");
            code.AppendLine();
            code.AppendLine("namespace Generated.Repositories");
            code.AppendLine("{");//1
            code.AppendLine($"\tpublic class {repositoryName}");
            code.AppendLine("\t{");//2
            code.AppendLine($"\t\tprivate readonly string _connectionString;");
            code.AppendLine();
            code.AppendLine($"\t\tpublic {repositoryName}(string connectionString)");
            code.AppendLine("\t\t{");
            code.AppendLine("\t\t\t_connectionString = connectionString;");
            code.AppendLine("\t\t}");
            code.AppendLine();

            string getByIdCode = GenerateGetById(table,options);
            if (!string.IsNullOrEmpty(getByIdCode))
            {
                code.AppendLine(getByIdCode);
                code.AppendLine();
            }

            code.AppendLine(GenerateGetAll(table, options));
            code.AppendLine();

            code.AppendLine(GenerateInsert(table, options));
            code.AppendLine();

            string updateCode = GenerateUpdate(table, options);
            if(!string.IsNullOrEmpty(updateCode))
            {
                code.AppendLine(updateCode);
                code.AppendLine();
            }

            string deleteCode = GenerateDelete(table, options);
            if (!string.IsNullOrEmpty(deleteCode))
            {
                code.AppendLine(deleteCode);
                code.AppendLine();
            }

            code.AppendLine(GenerateMapRow(table,options));
            code.AppendLine();

            //foreach (var foreignKey in table.ForeignKeys)
            //{
            //    code.AppendLine(Indent(GenerateGetByForeignKey(table, foreignKey), 2));
            //    code.AppendLine();
            //}


            code.AppendLine("\t}");//2
            code.AppendLine("}");//1
            return code.ToString();
        }

        private static string GenerateGetById( TableMetadata table,GeneratorOptions options)
        {
            List<ColumnMetadata> primaryKeyColumns = GetPrimaryKeyColumns(table);
            if (primaryKeyColumns.Count == 0) return null;
           
            string entityName = NamingHelper.ToEntityName(table.TableName);
            string methodParameters = GenerateMethodParameters(primaryKeyColumns, options);
            string whereClause = GeneratePrimaryKeyWhere(primaryKeyColumns);

            StringBuilder code = new StringBuilder();

            code.AppendLine($"\t\tpublic {entityName} GetById({methodParameters})");
           
            code.AppendLine("\t\t{");//start 1
            code.AppendLine($"\t\t\tstring query = @\"SELECT * FROM {QuoteTable(table)}");
            code.AppendLine($"\t\t\tWHERE {whereClause}\";");
            code.AppendLine();
            code.AppendLine($"\t\t\tusing(SqlConnection connection = new SqlConnection(_connectionString))");
            code.AppendLine("\t\t\t{");// start 2
            code.AppendLine("\t\t\t\tconnection.Open();");
            code.AppendLine();
            code.AppendLine("\t\t\t\tusing(SqlCommand command = new SqlCommand(query,connection))");
            code.AppendLine("\t\t\t\t{");//start 3

            foreach (ColumnMetadata column in primaryKeyColumns)
            {
                string propertyName = NamingHelper.ToPascalCase(column.Name);
                code.AppendLine(Indent(ParameterGenerator.Generate(column, propertyName),5));
            }
            code.AppendLine();
            code.AppendLine($"\t\t\t\t\tusing(SqlDataReader reader = command.ExecuteReader())");
            code.AppendLine("\t\t\t\t\t{");//start 4
            code.AppendLine($"\t\t\t\t\t\tif(!reader.Read()) return null;");
            code.AppendLine();
            code.AppendLine($"\t\t\t\t\t\t{entityName} entity = new {entityName}();");
            code.AppendLine();
            code.AppendLine("\t\t\t\t\t\tMapRow(reader, entity);");
            code.AppendLine();
            code.AppendLine("\t\t\t\t\t\treturn entity;");
            code.AppendLine("\t\t\t\t\t}");//end 4
            code.AppendLine("\t\t\t\t}");// end 3
            code.AppendLine("\t\t\t}");// end 2
            code.AppendLine("\t\t}");//end 1
            return code.ToString();
        }

        private static string GenerateInsert(TableMetadata table,GeneratorOptions options)
        {
            List<ColumnMetadata> insertColumns = GetInsertColumns(table);

            string entityName = NamingHelper.ToEntityName(table.TableName);

            string tableName = QuoteTable(table);

            ColumnMetadata identityColumn = GetIdentityColumn(table);

            StringBuilder code = new StringBuilder();

            // Method signature
            code.AppendLine($"\t\tpublic {entityName} Insert({entityName} entity, InsertOptions options = null)");

            code.AppendLine("\t\t{");

            // Validate entity
            code.AppendLine("\t\t\tif (entity == null)");
            code.AppendLine("\t\t\t\tthrow new ArgumentNullException(nameof(entity));");
            code.AppendLine();

            // Initialize options
            code.AppendLine("\t\t\tif (options == null)");
            code.AppendLine("\t\t\t\toptions = new InsertOptions();");
            code.AppendLine();

            // Validate requested default-value columns
            code.AppendLine(GenerateDefaultValueValidation(table));

            // Lists used to build the INSERT statement at runtime
            code.AppendLine("\t\t\tList<string> columns = new List<string>();");
            code.AppendLine("\t\t\tList<string> values = new List<string>();");
            code.AppendLine();

            // Connection
            code.AppendLine("\t\t\tusing (SqlConnection connection = new SqlConnection(_connectionString))");

            code.AppendLine("\t\t\t{");
            code.AppendLine("\t\t\t\tconnection.Open();");
            code.AppendLine();

            // Command
            code.AppendLine("\t\t\t\tusing (SqlCommand command = new SqlCommand())");

            code.AppendLine("\t\t\t\t{");
            code.AppendLine("\t\t\t\t\tcommand.Connection = connection;");
            code.AppendLine();

            // Add selected columns and parameters
            foreach (ColumnMetadata column in insertColumns)
            {
                string propertyName =
                    NamingHelper.ToPascalCase(column.Name);

                string escapedColumnName =
                    NamingHelper.EscapeCSharpStringLiteral(
                        EscapeSqlIdentifier(column.Name));

                string escapedOriginalColumnName =
                    NamingHelper.EscapeCSharpStringLiteral(column.Name);

                string escapedParameterName =
                    NamingHelper.EscapeCSharpStringLiteral(propertyName);

                code.AppendLine(
                    $"\t\t\t\t\tif (!options.UseDefaultValueFor.Contains(\"{escapedOriginalColumnName}\"))");

                code.AppendLine("\t\t\t\t\t{");

                code.AppendLine(
                    $"\t\t\t\t\t\tcolumns.Add(\"[{escapedColumnName}]\");");

                code.AppendLine(
                    $"\t\t\t\t\t\tvalues.Add(\"@{escapedParameterName}\");");

                code.AppendLine(Indent(ParameterGenerator.Generate(column,$"entity.{propertyName}"),6));

                code.AppendLine("\t\t\t\t\t}");
                code.AppendLine();
            }

            string escapedTableName = NamingHelper.EscapeCSharpStringLiteral(tableName);
            string escapedIdentitySqlName = null;
            if (identityColumn != null)
            {
                string identitySqlName = EscapeSqlIdentifier(identityColumn.Name);
                escapedIdentitySqlName = NamingHelper.EscapeCSharpStringLiteral(identitySqlName);
            }
           
            // Build SQL query
            code.AppendLine("\t\t\t\t\tStringBuilder queryBuild = new StringBuilder();");
            code.AppendLine($"\t\t\t\t\tqueryBuild.AppendLine(\"INSERT INTO {escapedTableName}\");");

            // No selected columns: DEFAULT VALUES
            code.AppendLine("\t\t\t\t\tif (columns.Count == 0)");
            code.AppendLine("\t\t\t\t\t{");

            if (identityColumn != null)
            {
                code.AppendLine($"\t\t\t\t\t\tqueryBuild.AppendLine(\"OUTPUT INSERTED.[{escapedIdentitySqlName}]\");");
            }
            
            code.AppendLine("\t\t\t\t\t\tqueryBuild.AppendLine(\"DEFAULT VALUES;\");");
            code.AppendLine("\t\t\t\t\t}");
            code.AppendLine("\t\t\t\t\telse");
            code.AppendLine("\t\t\t\t\t{");

            // Selected columns 
            code.AppendLine("\t\t\t\t\t\tqueryBuild.AppendLine($\"({string.Join(\", \", columns)})\");");

            if (identityColumn != null)
            {
                code.AppendLine($"\t\t\t\t\t\tqueryBuild.AppendLine(\"OUTPUT INSERTED.[{escapedIdentitySqlName}]\");");
            }

            // Selected values
            code.AppendLine("\t\t\t\t\t\tqueryBuild.AppendLine($\"VALUES({string.Join(\", \", values)});\");");

            code.AppendLine("\t\t\t\t\t}");
            code.AppendLine();
            code.AppendLine("\t\t\t\t\tstring query = queryBuild.ToString();");

            // Assign query to command
            code.AppendLine("\t\t\t\t\tcommand.CommandText = query;");
            code.AppendLine();

            // Execute INSERT
            if (identityColumn != null)
            {
                string identityProperty =
                    NamingHelper.ToPascalCase(identityColumn.Name);

                string identityType =
                    SqlToCSharpTypeMapper.Map(identityColumn.SqlType);

                string conversion =
                    GenerateScalarConversion(identityColumn, "scalarValue");

                code.AppendLine(
                    "\t\t\t\t\tobject scalarValue = command.ExecuteScalar();");

                code.AppendLine();

                code.AppendLine(
                    "\t\t\t\t\tif (scalarValue == null || scalarValue == DBNull.Value)");

                code.AppendLine("\t\t\t\t\t{");

                code.AppendLine(
                    "\t\t\t\t\t\tthrow new InvalidOperationException(\"The INSERT statement did not return an identity value.\");");

                code.AppendLine("\t\t\t\t\t}");
                code.AppendLine();

                code.AppendLine(
                    $"\t\t\t\t\t{identityType} generatedId = {conversion};");

                code.AppendLine();

                code.AppendLine(
                    $"\t\t\t\t\tentity.{identityProperty} = generatedId;");
            }
            else
            {
                code.AppendLine("\t\t\t\t\tcommand.ExecuteNonQuery();");
            }

            code.AppendLine();

            // Return inserted entity
            code.AppendLine("\t\t\t\t\treturn entity;");

            code.AppendLine("\t\t\t\t}");
            code.AppendLine("\t\t\t}");
            code.AppendLine("\t\t}");

            return code.ToString();
        }

        private static string GenerateUpdate(TableMetadata table, GeneratorOptions options)
        {
            List<ColumnMetadata> primaryKeyColumns = GetPrimaryKeyColumns(table);

            if (primaryKeyColumns.Count == 0) return null;

            List<ColumnMetadata> updateColumns = GetUpdateColumns(table);
            if (updateColumns.Count == 0)
            {
                throw new InvalidOperationException($"Table '{table.TableName}' does not have any updatetable columns.");
            }

            string entityName = NamingHelper.ToEntityName(table.TableName);
            string query = GenerateUpdateSql(table, updateColumns, primaryKeyColumns);
            StringBuilder code = new StringBuilder();

            code.AppendLine($"\t\tpublic bool Update({entityName} entity)");
            code.AppendLine("\t\t{");//start 1
            code.AppendLine("\t\t\tstring query = @\"");
            code.AppendLine($"{Indent(query, 3)}\";");
            code.AppendLine();
            code.AppendLine("\t\t\tusing(SqlConnection connection = new SqlConnection(_connectionString))");
            code.AppendLine("\t\t\t{");//start2
            code.AppendLine("\t\t\t\tconnection.Open();");
            code.AppendLine("\t\t\t\tusing(SqlCommand command = new SqlCommand(query, connection))");
            code.AppendLine("\t\t\t\t{");//start3

            foreach (ColumnMetadata column in updateColumns)
            {
                string propertyName = NamingHelper.ToPascalCase(column.Name);
                code.AppendLine(Indent(ParameterGenerator.Generate(column, $"entity.{propertyName}"), 5));
            }

            foreach (ColumnMetadata column in primaryKeyColumns)
            {
                string propertyName = NamingHelper.ToPascalCase(column.Name);
                code.AppendLine(Indent(ParameterGenerator.Generate(column, $"entity.{propertyName}"), 5));
            }
            code.AppendLine();
            code.AppendLine($"\t\t\t\t\tint rowsAffected = command.ExecuteNonQuery();");
            code.AppendLine();
            code.AppendLine($"\t\t\t\t\treturn rowsAffected > 0;");

            code.AppendLine("\t\t\t\t}");//end 3
            code.AppendLine("\t\t\t}");//end 2
            code.AppendLine("\t\t}");//end 1

            return code.ToString();
        }
        
        private static string GenerateDelete(TableMetadata table,GeneratorOptions options)
        {
            List<ColumnMetadata> primaryKeyColumns = GetPrimaryKeyColumns(table);
            if (primaryKeyColumns.Count == 0) return null;
          
            string methodParameters = GenerateMethodParameters(primaryKeyColumns,options);
            string query = GenerateDeleteSql(table, primaryKeyColumns);

            StringBuilder code = new StringBuilder();

            code.AppendLine($"\t\tpublic bool Delete({methodParameters})");
            code.AppendLine("\t\t{");//1
            code.AppendLine("\t\t\tstring query = @\"");
            code.AppendLine($"{Indent(query,3)}\";");
            code.AppendLine();
            code.AppendLine("\t\t\tusing(SqlConnection connection = new SqlConnection(_connectionString))");
            code.AppendLine("\t\t\t{");//2
            code.AppendLine("\t\t\t\tconnection.Open();");
            code.AppendLine("\t\t\t\tusing(SqlCommand command = new SqlCommand(query, connection))");
            code.AppendLine("\t\t\t\t{");//3

            foreach (ColumnMetadata column in primaryKeyColumns)
            {
                string propertyName = NamingHelper.ToPascalCase(column.Name);
                code.AppendLine(Indent(ParameterGenerator.Generate(column, propertyName), 5));
            }
            code.AppendLine();
            code.AppendLine("\t\t\t\t\tint rowsAffected = command.ExecuteNonQuery();");
            code.AppendLine();
            code.AppendLine($"\t\t\t\t\treturn rowsAffected > 0;");

            code.AppendLine("\t\t\t\t}");//3
            code.AppendLine("\t\t\t}");//2
            code.AppendLine("\t\t}");//1

            return code.ToString();
        }

        private static string GenerateGetAll(TableMetadata table,GeneratorOptions options)
        {
            string entityName = NamingHelper.ToEntityName(table.TableName);
            string tableName = QuoteTable(table);
            StringBuilder code = new StringBuilder();

            code.AppendLine($"\t\tpublic List<{entityName}> GetAll()");
            code.AppendLine("\t\t{");//start 1
            code.AppendLine($"\t\t\tList<{entityName}> entities = new List<{entityName}>();");
            code.AppendLine();
            code.AppendLine($"\t\t\tstring query = @\"SELECT * FROM {tableName};\";");
            code.AppendLine();
            code.AppendLine($"\t\t\tusing(SqlConnection connection = new SqlConnection(_connectionString))");
            code.AppendLine("\t\t\t{");// start 2
            code.AppendLine("\t\t\t\tconnection.Open();");
            code.AppendLine($"\t\t\t\tusing(SqlCommand command = new SqlCommand(query,connection))");
            code.AppendLine("\t\t\t\t{");//start 3
            code.AppendLine("\t\t\t\t\tusing(SqlDataReader reader = command.ExecuteReader())");
            code.AppendLine("\t\t\t\t\t{");//start 4
            code.AppendLine();
            code.AppendLine($"\t\t\t\t\t\twhile(reader.Read())");
            code.AppendLine("\t\t\t\t\t\t{");//start 5
            code.AppendLine($"\t\t\t\t\t\t\t{entityName} entity = new {entityName}();");
            code.AppendLine();
            code.AppendLine("\t\t\t\t\t\t\tMapRow(reader, entity);");
            /*
             * we can use this code:
             * $"{entityName} entity = ReaderMapper.Map<{entityName}>(reader);"
             */
            code.AppendLine();
            code.AppendLine($"\t\t\t\t\t\t\tentities.Add(entity);");
            code.AppendLine("\t\t\t\t\t\t}");//end 5
            code.AppendLine("\t\t\t\t\t}");//end 4
            code.AppendLine("\t\t\t\t}");// end 3)
            code.AppendLine("\t\t\t}");// end 2)
            code.AppendLine();
            code.AppendLine("\t\t\treturn entities;");
            code.AppendLine("\t\t}");//end 1

            return code.ToString();
        }

        //private static string GenerateGetByForeignKey(TableMetadata table,ForeignKeyMetadata foreignKey,GeneratorOptions options)
        //{
        //    if (foreignKey.Columns.Count == 0) return string.Empty;

        //    StringBuilder code = new StringBuilder();
        //    string className = NamingHelper.ToPascalCase(table.TableName);
        //    //Build method name
        //    StringBuilder methodName = new StringBuilder("GetBy");
        //    foreach(var fkColumn in foreignKey.Columns)
        //    {
        //        methodName.Append(NamingHelper.ToPascalCase(fkColumn.ParentColumn));
        //    }
        //    //Build method parameters
        //    StringBuilder methodParameters = new StringBuilder();
        //    foreach(var fkcColumn in foreignKey.Columns)
        //    {
        //        if(methodParameters.Length>0) methodParameters.Append(", ");
        //        ColumnMetadata column = FindColumn(table,fkcColumn.ParentColumn);
        //        string csharpType = SqlToCSharpTypeMapper.MapColumn(column, options);
        //        string parameterName = NamingHelper.ToPascalCase(column.Name);

        //        methodParameters.Append($"{csharpType} {parameterName}");
        //    }

        //    code.AppendLine($"public List<{className}> {methodName}({methodParameters})");
        //    code.AppendLine("{");//1
        //    code.AppendLine("\tstring query = @\"");
        //    code.AppendLine($"\tSELECT * FROM {QuoteTable(table)}");
        //    code.AppendLine("\tWHERE");
        //    for(int i = 0;i < foreignKey.Columns.Count;i++)
        //    {
        //        var fkColumn = foreignKey.Columns[i];
        //        if (i > 0) code.Append("\n\t AND \n");
        //        code.Append($"\t[{EscapeSqlIdentifier(fkColumn.ParentColumn)}] = @{fkColumn.ParentColumn}");
        //    }
        //    code.Append(";\";");
        //    code.AppendLine("\n");
        //    code.AppendLine("\tusing(SqlConnection connection = new SqlConnection(_connectionString))");
        //    code.AppendLine("\t{");//2
        //    code.AppendLine("\t\tconnection.Open();");
        //    code.AppendLine("\t\tusing(SqlCommand command = new SqlCommand(query, connection))");
        //    code.AppendLine("\t\t{");//3

        //    foreach(var fkcColumn in foreignKey.Columns)
        //    {
        //        ColumnMetadata column = FindColumn(table, fkcColumn.ParentColumn);
        //        string parameterName = NamingHelper.ToPascalCase(column.Name);
        //        code.AppendLine(Indent(ParameterGenerator.Generate(column, parameterName),3));
        //    }
        //    code.AppendLine();
        //    code.AppendLine($"\t\t\tList<{className}> result = new List<{className}>();");
        //    code.AppendLine();
        //    code.AppendLine("\t\t\tusing(SqlDataReader reader = command.ExecuteReader())");
        //    code.AppendLine("\t\t\t{");//4
        //    code.AppendLine("\t\t\t\twhile(reader.Read())");
        //    code.AppendLine("\t\t\t\t{");//5
        //    code.AppendLine($"\t\t\t\t\t{className} entity = new {className}();");
        //    code.AppendLine();
        //    foreach(ColumnMetadata column in table.Columns)
        //    {
        //        string propertyName = NamingHelper.ToPascalCase(column.Name);
        //        code.AppendLine(Indent(ReaderMapper.GenerateAssignment(column, propertyName,options),5));
        //    }
        //    code.AppendLine();
        //    code.AppendLine($"\t\t\t\t\tresult.Add(entity);");
        //    code.AppendLine("\t\t\t\t}");//5
        //    code.AppendLine("\t\t\t}");//4
        //    code.AppendLine();
        //    code.AppendLine("\t\t\treturn result;");
        //    code.AppendLine("\t\t}");//3
        //    code.AppendLine("\t}");//2
        //    code.AppendLine("}");//1
        //    return code.ToString();
        //}

        private static string GeneratePrimaryKeyWhere(List<ColumnMetadata> primaryKeyColumns)
        {
            List<string> conditions = new List<string>();
            foreach (ColumnMetadata column in primaryKeyColumns)
            {
                string propertyName = NamingHelper.ToPascalCase(column.Name);
                conditions.Add($"[{EscapeSqlIdentifier(column.Name)}] = @{propertyName}");
            }

            return string.Join(" AND ", conditions);
        }

        private static List<ColumnMetadata> GetPrimaryKeyColumns(TableMetadata table)
        {
            return table.Columns.FindAll(c => c.IsPrimaryKey).OrderBy(c => c.KeyOrdinal).ToList();
        }

        private static string GenerateMethodParameters(List<ColumnMetadata> columns,GeneratorOptions options)
        {
            List<string> parameters = new List<string>();
            foreach (ColumnMetadata column in columns)
            {
                string csharpType = SqlToCSharpTypeMapper.MapColumn(column, options);
                string propertyName = NamingHelper.ToPascalCase(column.Name);
                parameters.Add($"{csharpType} {propertyName}");
            }
            return string.Join(", ", parameters);
        }

        private static string GenerateDefaultValueValidation(TableMetadata table)
        {
            StringBuilder code= new StringBuilder();

            List<ColumnMetadata> defaultColumns = table.Columns.FindAll(c => c.HasDefaultValue && !c.IsIdentity && !c.IsComputed);

            code.AppendLine("\t\t\tforeach(string columnName in options.UseDefaultValueFor)");
            code.AppendLine("\t\t\t{");
            code.AppendLine("\t\t\t\tbool isAllowedDefaultColumn = false;");
            code.AppendLine();

            foreach(ColumnMetadata column in defaultColumns)
            {
                string escapedName = NamingHelper.EscapeCSharpStringLiteral(column.Name);
                code.AppendLine($"\t\t\t\tif (string.Equals(columnName, \"{escapedName}\", StringComparison.OrdinalIgnoreCase))");
                code.AppendLine("\t\t\t\t{");
                code.AppendLine($"\t\t\t\t\tisAllowedDefaultColumn = true;");
                code.AppendLine($"\t\t\t\t\tbreak;");
                code.AppendLine("\t\t\t\t}");
                code.AppendLine();
            }
            code.AppendLine("\t\t\t\tif(!isAllowedDefaultColumn)");
            code.AppendLine("\t\t\t\t{");
            code.AppendLine("\t\t\t\t\tthrow new ArgumentException(\"The selected column does not have a default value or cannot use DEFAULT: \" + columnName);");
            code.AppendLine("\t\t\t\t}");
            code.AppendLine("\t\t\t}");

            return code.ToString();
        }

        private static List<ColumnMetadata> GetInsertColumns(TableMetadata table)
        {
            return table.Columns.FindAll(c => !c.IsIdentity && !c.IsComputed);
        }

        private static string GenerateInsertColumnList(List<ColumnMetadata> columns)
        {
            List<string> columnNames = new List<string>();
            foreach (ColumnMetadata column in columns)
            {
                columnNames.Add($"[{EscapeSqlIdentifier(column.Name)}]");
            }
            return string.Join(", ", columnNames);
        }

        private static string GenerateInsertParameterList(List<ColumnMetadata> columns)
        {
            List<string> parameters = new List<string>();
            foreach (ColumnMetadata column in columns)
            {
                string propertyName = NamingHelper.ToPascalCase(column.Name);
                parameters.Add($"@{propertyName}");
            }
            return string.Join(", ", parameters);
        }

        private static ColumnMetadata GetIdentityColumn(TableMetadata table)
        {
            foreach (ColumnMetadata column in table.Columns)
            {
                if(column.IsIdentity) return column;
            }
            return null;
        }

        private static string GenerateScalarConversion(ColumnMetadata column, string expression)
        {
            string csharpType = SqlToCSharpTypeMapper.Map(column.SqlType);
            switch (csharpType)
            {
                case "Guid":
                    return $"Guid.Parse({expression}.ToString())";

                case "int":
                    return $"Convert.ToInt32({expression})";

                case "long":
                    return $"Convert.ToInt64({expression})";

                case "short":
                    return $"Convert.ToInt16({expression})";

                case "byte":
                    return $"Convert.ToByte({expression})";

                case "decimal":
                    return $"Convert.ToDecimal({expression})";

                case "double":
                    return $"Convert.ToDouble({expression})";

                case "float":
                    return $"Convert.ToSingle({expression})";

                default:
                    return $"({csharpType})Convert.ChangeType({expression}, typeof({csharpType}))";
            }
        }

        private static List<ColumnMetadata> GetUpdateColumns(TableMetadata table)
        {
            return table.Columns.FindAll(c => !c.IsPrimaryKey && !c.IsIdentity && !c.IsComputed);
        }

        private static string GenerateUpdateSetClause(List<ColumnMetadata> updateColumns)
        {
            List<string> assignments = new List<string>();
            foreach (ColumnMetadata column in updateColumns)
            {
                string propertyName = NamingHelper.ToPascalCase(column.Name);
                assignments.Add($"[{EscapeSqlIdentifier(column.Name)}] = @{propertyName}");
            }
            return string.Join(", ", assignments);
        }

        private static string GenerateUpdateSql(TableMetadata table,List<ColumnMetadata> updateColumns,List<ColumnMetadata> primaryKeyColumns)
        {
            string tableName = QuoteTable(table);
            string setClause = GenerateUpdateSetClause(updateColumns);
            string whereClause = GeneratePrimaryKeyWhere(primaryKeyColumns);

            return $"UPDATE {tableName}\r\nSET {setClause}\r\nWHERE {whereClause};";
        }

        private static string GenerateDeleteSql(TableMetadata table,List<ColumnMetadata> primaryKeyColumns)
        {
            string tableName = QuoteTable(table);
            string whereClause = GeneratePrimaryKeyWhere(primaryKeyColumns);

            return $"DELETE FROM {tableName} \nWHERE {whereClause};";
        }

        private static string QuoteTable(TableMetadata table)
        {
            return $"[{EscapeSqlIdentifier(table.SchemaName)}].[{EscapeSqlIdentifier(table.TableName)}]";
        }

        private static string EscapeSqlIdentifier(string name)
        {
            return name.Replace("]", "]]");
        }

        private static string removeEscaper(string name)
        {
            return name.Replace("]", "");
        }

        private static string GetSqlIdentityType(ColumnMetadata column)
        {
            switch(column.SqlType.ToLower())
            {
                case "bigint":
                    return "BIGINT";
                case "int":
                    return "INT";
                case "smallint":
                    return "SMALLINT";
                case "tinyint":
                    return "TINYINT";
                default:
                    throw new InvalidOperationException($"Identity type '{column.SqlType}' is not supported.");

            }
        }

        private static ColumnMetadata FindColumn(TableMetadata table, string columnName)
        {
            ColumnMetadata column = table.Columns.Find(c => c.Name == columnName);
            if (column == null)
            {
                throw new InvalidOperationException($"Column '{columnName}' was not found in table '{table.TableName}'.");
            }

            return column;
        }

        private static string GenerateMapRow(TableMetadata table,GeneratorOptions options)
        {
            string entityName = NamingHelper.ToEntityName(table.TableName);

            StringBuilder code = new StringBuilder();

            code.AppendLine($"\t\tprivate static void MapRow(SqlDataReader reader, {entityName} entity)");
            code.AppendLine("\t\t{");

            for (int i = 0; i < table.Columns.Count; i++)
            {
                ColumnMetadata column = table.Columns[i];

                string propertyName = NamingHelper.ToPascalCase(column.Name);

                string assignment = ReaderMapper.GenerateAssignment(column, propertyName, options, i);

                code.AppendLine(Indent(assignment, 3));
            }

            code.AppendLine("\t\t}");

            return code.ToString();
        }

        private static string Indent(string text, int level)
        {
            string prefix = new string(' ', level * 4);
            string[] lines = text.Split(new[] {"\n","\r\n"}, StringSplitOptions.None);
            StringBuilder result = new StringBuilder();
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) result.AppendLine();
                else result.AppendLine(prefix + line);
            }

            return result.ToString().TrimEnd();
        }

    }   
}
