using CrudCodeGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Metadata
{
    public static class RelationshipMetadataBuilder
    {
        public static List<RelationshipMetadata> Build(DatabaseMetadata databaseMetadata)
        {
            var relationships = new List<RelationshipMetadata>();
            foreach (TableMetadata table in databaseMetadata.Tables)
            {
                foreach (ForeignKeyMetadata foreignKey in table.ForeignKeys)
                {
                    relationships.Add(BuildRelationship(table,foreignKey));
                }
            }
            return relationships;
        }

        private static RelationshipMetadata BuildRelationship(TableMetadata table, ForeignKeyMetadata foreignKey)
        {
            var relationship = new RelationshipMetadata();

            relationship.ForeignKeyName = foreignKey.Name;
            relationship.SourceSchema = foreignKey.ParentSchema;
            relationship.SourceTable = foreignKey.ParentTable;
            relationship.TargetSchema = foreignKey.ReferencedSchema;
            relationship.TargetTable = foreignKey.ReferencedTable;
            relationship.Columns.AddRange(foreignKey.Columns);
            relationship.Cardinality = IsForeignKeyUnique(table, foreignKey) ?
                RelationshipCardinality.OneToOne : RelationshipCardinality.OneToMany;
            relationship.Optionality = GetOptionality(table, foreignKey);

            return relationship;
        }

        private static bool IsForeignKeyUnique(TableMetadata table,ForeignKeyMetadata foreignKey)
        {
            var foreignKeyColumns = new HashSet<string>(foreignKey.Columns.ConvertAll(c => c.ParentColumn), StringComparer.OrdinalIgnoreCase);

            foreach(var uniqueKey in table.UniqueKeys)
            {
                var uniqueColumns = new HashSet<string>(uniqueKey.Columns, StringComparer.OrdinalIgnoreCase);

                if (foreignKeyColumns.SetEquals(uniqueColumns))
                    return true;
            }
            return false;
        }

        private static RelationshipOptionality GetOptionality(TableMetadata table, ForeignKeyMetadata foreignKey)
        {
            foreach(var fkColumn in foreignKey.Columns)
            {
                ColumnMetadata column = table.Columns.Find(c => string.Equals(c.Name,fkColumn.ParentColumn,StringComparison.OrdinalIgnoreCase));
                if (column == null)
                {
                    throw new InvalidOperationException($"Column '{fkColumn.ParentColumn}'" +
                        $" was not found in table '{table.TableName}'. ");
                }

                if (column.IsNullable) return RelationshipOptionality.Optional;
            }
            return RelationshipOptionality.Required;
        }

    }
}
