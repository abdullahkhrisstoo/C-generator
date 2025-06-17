using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seagull.FrameWork.Repository.ModelCodeGenerator
{
   public class MapperGenerator
    {
        public static async Task GenerateMapperProfileAsync(Type modelType, string nameSpace, string outputPath)
        {
            var modelName = modelType.Name;
            var sb = new StringBuilder();
            sb.AppendLine("using AutoMapper;");
            sb.AppendLine($"using {nameSpace};");
            sb.AppendLine($"using {nameSpace}.DTOs;");
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace}.Profiles");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {modelName}Profile : Profile");
            sb.AppendLine("    {");
            sb.AppendLine($"        public {modelName}Profile()");
            sb.AppendLine("        {");
            sb.AppendLine($"            CreateMap<{modelName}, {modelName}Dto>().ReverseMap();;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            await FileWriterGenerator.WriteToFileAsync(Path.Combine(outputPath, $"{modelName}Profile.cs"), sb.ToString());
        }
    }
}
