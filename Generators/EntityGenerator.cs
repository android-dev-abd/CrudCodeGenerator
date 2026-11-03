using CrudCodeGenerator.Configuration;
using CrudCodeGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Generators
{
    public static class EntityGenerator
    {
        public static string Generate(TableMetadata table, List<RelationshipMetadata> relationships,GeneratorOptions options)
        {
            StringBuilder code = new StringBuilder();
            int selfReferenceCount = CountSelfReferences(table, relationships);
            HashSet<string> propertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string entityName = NamingHelper.ToEntityName(table.TableName);
           


            code.AppendLine("using System;");
            code.AppendLine("using System.Collections.Generic;");
            code.AppendLine();
            code.AppendLine("namespace Generated.Entities");
            code.AppendLine("{");
            code.AppendLine($"\tpublic class {entityName}");
            code.AppendLine("\t{");

            foreach (ColumnMetadata column in table.Columns)
            {
                string propertyName = NamingHelper.ToPascalCase(column.Name);
                propertyNames.Add(propertyName);
                string csharpType = SqlToCSharpTypeMapper.MapColumn(column,options);
                code.AppendLine($"\t\tpublic {csharpType} {propertyName} {{ get; set; }}");
                code.AppendLine();
            }

            foreach (var relationship in relationships)
            {
                if (!IsSourceTable(relationship, table))
                {
                    continue;
                }
                string navigation = GenerateSourceNavigation(relationship, propertyNames);
                code.AppendLine($"\t\t{navigation}");
            }

            foreach (var relationship in relationships)
            {
                if (!IsTargetTable(relationship, table))
                {
                    continue;
                }
                string navigation = GenerateTargetNavigation(relationship,selfReferenceCount, propertyNames);
                code.AppendLine($"\t\t{navigation}");
            }

            code.AppendLine("\t}");
            code.AppendLine("}");

            return code.ToString();
        }

        private static string GenerateSourceNavigation(RelationshipMetadata relationship,HashSet<string> propertyNames)
        {
            string targetEntity = NamingHelper.ToEntityName(relationship.TargetTable);
            string propertyName = NavigationPropertyNamingStrategy.GetSourceNavigationName(relationship, targetEntity,propertyNames);
            return $"public {targetEntity} {propertyName} {{ get; set; }}";
        }

        private static string GenerateTargetNavigation(RelationshipMetadata relationship,int selfReferenceCount, HashSet<string> propertyNames)
        {
            string sourceEntity = NamingHelper.ToEntityName(relationship.SourceTable);
            string propertyName = NavigationPropertyNamingStrategy.GetTargetNavigationName(relationship, sourceEntity, selfReferenceCount,propertyNames);


            if (relationship.Cardinality == RelationshipCardinality.OneToMany)
            {
                return $"public List<{sourceEntity}> {propertyName} {{ get; set; }}";
            }
                       
            return $"public {sourceEntity} {propertyName} {{ get; set; }}";
        }

        private static int CountSelfReferences(TableMetadata table, List<RelationshipMetadata> relationships)
        {
            return relationships.Count(r => r.IsSelfReference && r.SourceSchema.Equals(table.SchemaName,StringComparison.OrdinalIgnoreCase)
            && r.SourceTable.Equals(table.TableName,StringComparison.OrdinalIgnoreCase));
        }

        private static string Indent(string text, int level)
        {
            string prefix = new string(' ', level * 4);
            string[] lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            StringBuilder result = new StringBuilder();
            foreach (string line in lines)
            {
                if (line.Length == 0) result.AppendLine();
                else result.AppendLine(prefix + line);
            }

            return result.ToString();
        }

        private static bool IsSourceTable(RelationshipMetadata relationship,TableMetadata table)
        {
            return relationship.SourceSchema.Equals(table.SchemaName, StringComparison.OrdinalIgnoreCase)
                    && relationship.SourceTable.Equals(table.TableName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsTargetTable(RelationshipMetadata relationship,TableMetadata table)
        {
            return relationship.TargetSchema.Equals(table.SchemaName, StringComparison.OrdinalIgnoreCase)
                    && relationship.TargetTable.Equals(table.TableName, StringComparison.OrdinalIgnoreCase);
        }

       
      
    }
}
