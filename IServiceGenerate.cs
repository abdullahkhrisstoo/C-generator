using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seagull.FrameWork.Repository.ModelCodeGenerator
{
   public class IServiceGenerate
    {
        public static async Task GenerateIServiceAsync(Type modelType, string nameSpace, string outputPath)
        {
            var modelName = modelType.Name;
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace}.Services.Interfaces");
            sb.AppendLine("{");
            sb.AppendLine($"    public interface I{modelName}Service");
            sb.AppendLine("    {");
            sb.AppendLine($"        Task<{modelName}Dto> GetById(int id);");
            sb.AppendLine($"        Task<PaginatedResult<{modelName}Dto>> GetAll(int pageNumber, int pageSize);");
            sb.AppendLine($"        Task<{modelName}Dto> Create({modelName}Dto dto);");
            sb.AppendLine($"        Task<{modelName}Dto> Update({modelName}Dto dto);");
            sb.AppendLine($"        Task<bool> Delete(int id);");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            await FileWriterGenerator.WriteToFileAsync(Path.Combine(outputPath, $"I{modelName}Service.cs"), sb.ToString());
        }

    }
}
