using CrudCodeGenerator.Configuration;
using CrudCodeGenerator.Generators;
using CrudCodeGenerator.Metadata;
using CrudCodeGenerator.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;


namespace CrudCodeGenerator
{
    public class Program
    {
        private static void GenerateFiles(TableMetadata table, List<RelationshipMetadata> relationships, string outputDirectory,GeneratorOptions options)
        {
            string entityName = NamingHelper.ToEntityName(table.TableName);

            string entitiesDirectory = System.IO.Path.Combine(outputDirectory, "Entities");
            string repositoriesDirectory = System.IO.Path.Combine(outputDirectory, "Repositories");

            string entityCode = EntityGenerator.Generate(table, relationships,options);
            CodeFileWriter.Write(entitiesDirectory, entityName + ".cs", entityCode);

            string repositoryCode = RepositoryGenerator.Generate(table, options);
            CodeFileWriter.Write(repositoriesDirectory, entityName + "Repository.cs", repositoryCode);

        }

        private static void GenerateAllFiles(DatabaseMetadata databaseMetadata, List<RelationshipMetadata> relationships, string outputDirectory,GeneratorOptions options)
        {
            foreach (TableMetadata table in databaseMetadata.Tables)
            {
                GenerateFiles(table, relationships, outputDirectory,options);
            }
        }
        static void Main(string[] args)
        {
            GeneratorOptions options = new GeneratorOptions
            {
                CSharpLanguageVersion = "7.3",
                EnableNullableReferenceTypes = false
            };
            string connectionString = "server=.;database=db2;Trusted_Connection=True;";

            string outputDirectory = @"C:\GeneratedCode";

            DatabaseMetadataReader databaseMetadataReader = new DatabaseMetadataReader(connectionString);

            DatabaseMetadata databaseMetadata = databaseMetadataReader.Read();

            List<RelationshipMetadata> relationships = RelationshipMetadataBuilder.Build(databaseMetadata);

            GenerateAllFiles(databaseMetadata, relationships, outputDirectory,options);


            Console.WriteLine("done");


        }
    }
}
