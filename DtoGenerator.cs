using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Seagull.FrameWork.Repository.ModelCodeGenerator
{
   public class DtoGenerator
    {
        public static async Task GenerateDtoAsync(Type modelType, List<PropertyInfo> properties, string nameSpace, string outputPath)
        {
            var modelName = modelType.Name;
            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace}.DTOs");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {modelName}Dto");
            sb.AppendLine("    {");

            foreach (var prop in properties)
            {
                if (!IsNavigationProperty(prop))
                {
                    sb.AppendLine($"        public {GetDtoPropertyType(prop)} {prop.Name} {{ get; set; }}");
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            await FileWriterGenerator.WriteToFileAsync(Path.Combine(outputPath, $"{modelName}Dto.cs"), sb.ToString());
        }

        private static string GetDtoPropertyType(PropertyInfo prop)
        {
            var type = prop.PropertyType;
            var typeName = type.Name;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                return $"{underlyingType.Name}?";
            }

            return typeName switch
            {
                "Int32" => "int",
                "String" => "string",
                "Boolean" => "bool",
                "Decimal" => "decimal",
                "Double" => "double",
                "Single" => "float",
                "Int64" => "long",
                "DateTime" => "DateTime",
                "Guid" => "Guid",
                _ => typeName
            };
        }
        private static bool IsNavigationProperty(PropertyInfo prop)
        {
            var type = prop.PropertyType;
            return (type.IsClass && type != typeof(string)) ||
                   (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>));
        }
    }
}
