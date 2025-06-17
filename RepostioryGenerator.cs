using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Seagull.FrameWork.Repository.ModelCodeGenerator
{
    public class RepostioryGenerator
    {
        public static async Task GenerateRepositoryAsync(Type modelType, string nameSpace, string outputPath)
        {
            var modelName = modelType.Name;
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Linq.Expressions;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine($"using {nameSpace};");
            sb.AppendLine($"using {nameSpace}.Repositories.Interfaces;");
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace}.Repositories.Implementations");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {modelName}Repository : GenericRepository<{modelName}>, I{modelName}Repository, IScopedDependency");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly SeagullDbContext _dbContext;");
            sb.AppendLine();
            sb.AppendLine($"        public {modelName}Repository(SeagullDbContext dbContext) : base(dbContext)");
            sb.AppendLine("        {");
            sb.AppendLine("            _dbContext = dbContext;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            await FileWriterGenerator.WriteToFileAsync(Path.Combine(outputPath, $"{modelName}Repository.cs"), sb.ToString());
        }

    }
}
