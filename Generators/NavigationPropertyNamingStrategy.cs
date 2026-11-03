using CrudCodeGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrudCodeGenerator.Generators
{
    public static class NavigationPropertyNamingStrategy
    {
        public static string GetSourceNavigationName(RelationshipMetadata relationship, string targetEntity,HashSet<string> usedNames)
        {
            string proposedName = GetNavigationNameFromForeignKey(relationship);

            if (string.IsNullOrWhiteSpace(proposedName)) proposedName = targetEntity;
               

            return GetUniqueName(proposedName, usedNames);
        }

        public static string GetTargetNavigationName(RelationshipMetadata relationship, string sourceEntity,int selfReferenceCount,HashSet<string> usedNames)
        {
            string proposedName;

            if(relationship.IsSelfReference)
            {
                proposedName = GetSelfReferenceTargetName(relationship,sourceEntity,selfReferenceCount);
            }
            else
            {
                proposedName = GetNormalTargetNavigationName(relationship, sourceEntity);
            }

            if(relationship.Cardinality == RelationshipCardinality.OneToMany)
            {
                proposedName = NamingHelper.ToPlural(proposedName);
            }

            return GetUniqueName(proposedName,usedNames);
        }

        private static string GetNormalTargetNavigationName(RelationshipMetadata relationship, string sourceEntity)
        {
            if (relationship.Columns.Count == 1)
            {
                string parentColumn = relationship.Columns[0].ParentColumn;
                string name = NamingHelper.ToPascalCase(parentColumn);
                name = NamingHelper.RemoveIdSuffix(name);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    string targetEntity = NamingHelper.ToEntityName(relationship.TargetTable);
                    if (name.Equals(targetEntity, StringComparison.OrdinalIgnoreCase))
                    { return sourceEntity; }
                    return name + sourceEntity;
                }
            }
            return sourceEntity;
        }

        private static string GetSelfReferenceTargetName(RelationshipMetadata relationship, string sourceEntity, int selfReferenceCount)
        {
            if (selfReferenceCount == 1)
                return sourceEntity;

            string name = GetNavigationNameFromForeignKey(relationship);
            if (!string.IsNullOrWhiteSpace(name))
                return name + sourceEntity;

            return sourceEntity;
        }

        public static string GetNavigationNameFromForeignKey(RelationshipMetadata relationship)
        {
            if (relationship.Columns.Count != 1) return null;

            string columnName = relationship.Columns[0].ParentColumn;
            string name = NamingHelper.ToPascalCase(columnName);
            return NamingHelper.RemoveIdSuffix(name);
        }

        private static string GetUniqueName(string proposedName, HashSet<string> usedNames)
        {
            if(usedNames.Add(proposedName)) return proposedName;

            int counter = 2;
            while(true)
            {
                string candidate = proposedName + counter;
                if(usedNames.Add(candidate))
                    return candidate;

                counter++;
            }
        }

    }
}
