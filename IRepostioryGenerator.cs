using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seagull.FrameWork.Repository.ModelCodeGenerator
{
    public class IRepostioryGenerator
    {
        public static async Task GenerateIRepositoryAsync(Type modelType, string nameSpace, string outputPath)
        {
            var modelName = modelType.Name;
            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq.Expressions;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine($"using {nameSpace};");
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace}.Repositories.Interfaces");
            sb.AppendLine("{");
            sb.AppendLine($"    public interface I{modelName}Repository : IGenericRepository<{modelName}> ");
            sb.AppendLine("    {");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            await FileWriterGenerator.WriteToFileAsync(Path.Combine(outputPath, $"I{modelName}Repository.cs"), sb.ToString());
        }
    }
}
