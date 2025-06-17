using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Seagull.FrameWork.Repository.ModelCodeGenerator
{
    public  class ConfigGeneator
    {

        public static async Task GenerateConfigFileAsync(Type modelType, List<PropertyInfo> properties, string nameSpace, string outputPath)
        {
            var modelName = modelType.Name;
            var sb = new StringBuilder();

            sb.AppendLine("using Microsoft.EntityFrameworkCore.Metadata.Builders;");
            sb.AppendLine($"using {nameSpace};");
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace}.Config");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {modelName}Config : IEntityTypeConfiguration<{modelName}>");
            sb.AppendLine("    {");
            sb.AppendLine($"        public void Configure(EntityTypeBuilder<{modelName}> builder)");
            sb.AppendLine("        {");
            sb.AppendLine($"            builder.ToTable(\"{Pluralize(modelName)}\");");
            sb.AppendLine("            builder.HasKey(e => e.Id);");
            sb.AppendLine();

            foreach (var prop in properties)
            {
                if (IsNavigationProperty(prop))
                {
                    GenerateRelationshipConfig(sb, prop, modelName);
                }
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            await FileWriterGenerator.WriteToFileAsync(Path.Combine(outputPath, $"{modelName}Configuration.cs"), sb.ToString());
        }

        public static void GenerateRelationshipConfig(StringBuilder sb, PropertyInfo property, string modelName)
        {
            var propType = property.PropertyType;
            var propName = property.Name;

            // Handle collection properties (one-to-many)
            if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                var relatedType = propType.GetGenericArguments()[0];
                var foreignKeyProperty = relatedType.GetProperties()
                    .FirstOrDefault(p => p.Name == $"{modelName}Id" || p.Name == $"{modelName}ID");

                if (foreignKeyProperty != null)
                {
                    sb.AppendLine($"            builder.HasMany(x => x.{propName})");
                    sb.AppendLine($"                   .WithOne()");
                    sb.AppendLine($"                   .HasForeignKey(x => x.{foreignKeyProperty.Name})");
                    sb.AppendLine($"                   .OnDelete(DeleteBehavior.Cascade);");
                }
                else
                {
                    sb.AppendLine($"            // Could not find foreign key for {propName} relationship");
                }
                sb.AppendLine();
            }
            else if (!propType.IsGenericType && propType.IsClass && propType != typeof(string))
            {
                var fkPropertyName = $"{propName}Id";
                var fkProperty = property.DeclaringType.GetProperty(fkPropertyName);

                if (fkProperty != null)
                {
                    sb.AppendLine($"            builder.HasOne(x => x.{propName})");
                    sb.AppendLine($"                   .WithMany()");
                    sb.AppendLine($"                   .HasForeignKey(x => x.{fkPropertyName})");
                    sb.AppendLine($"                   .OnDelete(DeleteBehavior.SetNull);");
                }
                else
                {
                    sb.AppendLine($"            // Could not find foreign key for {propName} relationship");
                }
                sb.AppendLine();
            }
        }



        private static bool IsNavigationProperty(PropertyInfo prop)
        {
            var type = prop.PropertyType;
            return (type.IsClass && type != typeof(string)) ||
                   (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>));
        }

        private static string Pluralize(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            if (name.EndsWith("y") && !name.EndsWith("ey"))
                return name.Substring(0, name.Length - 1) + "ies";
            if (name.EndsWith("s") || name.EndsWith("x") || name.EndsWith("z") ||
                name.EndsWith("ch") || name.EndsWith("sh"))
                return name + "es";
            return name + "s";
        }
    }
}
