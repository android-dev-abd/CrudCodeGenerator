using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CrudCodeGenerator.Models;
using System.Linq;
using CrudCodeGenerator.Generators;


namespace CrudCodeGenerator.Metadata
{
    public  class DatabaseMetadataReader
    {
        private readonly string _connectionString;

        public DatabaseMetadataReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<TableMetadata> GetTables()
        {
            Dictionary<string, TableMetadata> tables = new Dictionary<string, TableMetadata>();

            string sql = @"SELECT
                                s.name AS SchemaName,
                                t.name AS TableName,
                                c.name AS ColumnName,
                                ty.name AS SqlType,
                                c.max_length AS MaxLength,
                                c.precision AS Precision,
                                c.scale AS Scale,
                                c.is_nullable AS IsNullable,
                                c.is_identity AS IsIdentity,
                                c.is_computed AS IsComputed,
                                c.default_object_id AS HasDefaultValue,
                                CASE
                                    WHEN EXISTS (
                                        SELECT 1
                                        FROM sys.indexes i
                                        INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                                        WHERE i.is_primary_key = 1 AND ic.object_id = c.object_id AND ic.column_id = c.column_id
                                    ) THEN CAST(1 AS BIT)
                                    ELSE CAST(0 AS BIT)
                                END AS IsPrimaryKey 
                        From sys.tables t INNER JOIN sys.schemas s
                                ON t.schema_id = s.schema_id
                                          INNER JOIN sys.columns c
                                ON t.object_id = c.object_id
                                          INNER JOIN sys.types ty
                                ON c.user_type_id = ty.user_type_id
                        ORDER BY s.name, t.name, c.column_id;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string schemaName = reader["SchemaName"].ToString();
                            string tableName = reader["TableName"].ToString();
                            string columnName = reader["ColumnName"].ToString();

                            string key = $"{schemaName}.{tableName}";
                            if (!tables.ContainsKey(key))
                            {
                                tables[key] = new TableMetadata
                                {
                                    SchemaName = schemaName,
                                    TableName = tableName,
                                };
                            }

                            ColumnMetadata column = new ColumnMetadata
                            {
                                Name = columnName,
                                SqlType = reader["SqlType"].ToString(),
                                MaxLength = Convert.ToInt32(reader["MaxLength"]),
                                Precision = Convert.ToByte(reader["Precision"]),
                                Scale = Convert.ToByte(reader["Scale"]),
                                IsNullable = Convert.ToBoolean(reader["IsNullable"]),
                                IsIdentity = Convert.ToBoolean(reader["IsIdentity"]),
                                IsComputed = Convert.ToBoolean(reader["IsComputed"]),
                                IsPrimaryKey = Convert.ToBoolean(reader["IsPrimaryKey"]),
                                HasDefaultValue = Convert.ToInt32(reader["HasDefaultValue"]) != 0
                            };
                            tables[key].Columns.Add(column);
                        }
                    }
                }
            }
            return new List<TableMetadata>(tables.Values);
        }

        public  List<ForeignKeyMetadata> GetForeignKeys()
        {
            var foreignKeys = new Dictionary<int, ForeignKeyMetadata>();

            string query = @"SELECT	fk.object_id AS ForeignKeyId,
		                            fk.name AS ForeignKeyName,
		                            ps.name AS ParentSchema,
	                                pt.name AS ParentTable,
	                                rs.name AS ReferencedSchema,
		                            rt.name AS ReferencedTable,
		                            pc.name AS ParentColumn,
		                            rc.name AS ReferencedColumn
                            FROM    sys.foreign_keys fk
                                        INNER JOIN sys.foreign_key_columns fkc
                                        ON fk.object_id = fkc.constraint_object_id
						                INNER JOIN sys.tables pt
                                        ON fkc.parent_object_id = pt.object_id
						                INNER JOIN sys.schemas ps 
                                        ON pt.schema_id = ps.schema_id
						                INNER JOIN sys.columns pc 
                                        ON fkc.parent_object_id = pc.object_id and fkc.parent_column_id = pc.column_id
						                INNER JOIN sys.tables rt 
                                        ON fkc.referenced_object_id = rt.object_id
						                INNER JOIN sys.schemas rs 
                                        ON rt.schema_id = rs.schema_id
						                INNER JOIN sys.columns rc 
                                        ON fkc.referenced_object_id = rc.object_id and fkc.referenced_column_id = rc.column_id
                            ORDER BY ps.name,pt.name,fk.name,fkc.constraint_column_id;";
            using(SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using(SqlCommand command = new SqlCommand(query, connection))
                {    
                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            int foreignKeyId = Convert.ToInt32(reader["ForeignKeyId"]);
                            if(!foreignKeys.ContainsKey(foreignKeyId))
                            {
                                foreignKeys[foreignKeyId] = new ForeignKeyMetadata
                                {
                                    Name = reader["ForeignKeyName"].ToString(),
                                    ParentSchema = reader["ParentSchema"].ToString(),
                                    ParentTable = reader["ParentTable"].ToString(),
                                    ReferencedSchema = reader["ReferencedSchema"].ToString(),
                                    ReferencedTable = reader["ReferencedTable"].ToString(),
                                };
                            }

                            var column = new ForeignKeyColumnMetadata
                            {
                                ParentColumn = reader["ParentColumn"].ToString(),
                                ReferencedColumn = reader["ReferencedColumn"].ToString()
                            };

                            foreignKeys[foreignKeyId].Columns.Add(column);

                        }
                    }
                }
            }

            return new List<ForeignKeyMetadata>(foreignKeys.Values);
        }

        public  List<UniqueKeyMetadata> GetUniqueKeys()
        {
            string query = @"
            SELECT
                s.name AS SchemaName,
                t.name AS TableName,
                i.name AS IndexName,
                i.is_unique_constraint,
                c.name AS ColumnName,
                ic.key_ordinal
            FROM sys.tables t
            INNER JOIN sys.schemas s
                ON t.schema_id = s.schema_id
            INNER JOIN sys.indexes i
                ON t.object_id = i.object_id
            INNER JOIN sys.index_columns ic
                ON i.object_id = ic.object_id
                AND i.index_id = ic.index_id
            INNER JOIN sys.columns c
                ON ic.object_id = c.object_id
                AND ic.column_id = c.column_id
            WHERE
                i.is_unique = 1
                AND i.is_primary_key = 0
                AND ic.key_ordinal > 0
            ORDER BY
                s.name,
                t.name,
                i.name,
                ic.key_ordinal;";

            var result = new List<UniqueKeyMetadata>();

            var lookup = new Dictionary<string,UniqueKeyMetadata>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string schemaName = reader.GetString(reader.GetOrdinal("SchemaName"));
                            string tableName = reader.GetString(reader.GetOrdinal("TableName"));
                            string indexName = reader.GetString(reader.GetOrdinal("IndexName"));
                            string columnName = reader.GetString(reader.GetOrdinal("ColumnName"));
                            bool isUniqueConstraint = reader.GetBoolean(reader.GetOrdinal("is_unique_constraint"));

                            string key = $"{schemaName}.{tableName}.{indexName}";

                            if (!lookup.TryGetValue(key, out UniqueKeyMetadata uniqueKey))
                            {
                                uniqueKey = new UniqueKeyMetadata
                                {
                                    Name = indexName,
                                    SchemaName = schemaName,
                                    TableName = tableName,
                                    IsUniqueConstraint = isUniqueConstraint,
                                };
                                lookup.Add(key, uniqueKey);
                                result.Add(uniqueKey);
                            }
                            uniqueKey.Columns.Add(columnName);
                        }
                    }
                }
            }
            return result;
        }

        public DatabaseMetadata Read()
        {
            var databaseMetadata = new DatabaseMetadata();
            databaseMetadata.Tables = GetTables();

            var foreignKeys = GetForeignKeys();

            var uniqueKeys = GetUniqueKeys();

            foreach(var foreignKey in foreignKeys)
            {
                TableMetadata table = databaseMetadata.Tables.Find(t => t.SchemaName == foreignKey.ParentSchema
                && t.TableName == foreignKey.ParentTable);

                if(table != null)
                {
                    table.ForeignKeys.Add(foreignKey);
                }
            }

            foreach(var uniqueKey in uniqueKeys)
            {
                TableMetadata table = databaseMetadata.Tables.Find(t => t.SchemaName == uniqueKey.SchemaName
                 && t.TableName == uniqueKey.TableName);

                table?.UniqueKeys.Add(uniqueKey);
            }
            
            return databaseMetadata;
        }

        

    }
}
