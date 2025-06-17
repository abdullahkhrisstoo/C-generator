using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seagull.FrameWork.Repository.ModelCodeGenerator
{
   public class ServiceGenerate
    {
        public static async Task GenerateServiceAsync(Type modelType, string nameSpace, string outputPath)
        {
            var modelName = modelType.Name;
            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using AutoMapper;");
            sb.AppendLine($"using {nameSpace};");
            sb.AppendLine($"using {nameSpace}.DTOs;");
            sb.AppendLine($"using {nameSpace}.Repositories.Interfaces;");
            sb.AppendLine($"using {nameSpace}.Services.Interfaces;");
            sb.AppendLine($"using {nameSpace}.Infrastructure.UnitOfWork;");
            sb.AppendLine($"using {nameSpace}.Infrastructure.Dependencies;");
            sb.AppendLine($"using {nameSpace}.Common.Models;");
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace}.Services.Implementations");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {modelName}Service : I{modelName}Service, IScopedDependency");
            sb.AppendLine("    {");
            sb.AppendLine($"        private readonly I{modelName}Repository _Repository;");
            sb.AppendLine("        private readonly IUnitOfWork _unitOfWork;");
            sb.AppendLine("        private readonly IMapper _mapper;");
            sb.AppendLine();
            sb.AppendLine($"        public {modelName}Service(IUnitOfWork unitOfWork, I{modelName}Repository Repository, IMapper mapper)");
            sb.AppendLine("        {");
            sb.AppendLine("            _unitOfWork = unitOfWork;");
            sb.AppendLine("            _Repository = Repository;");
            sb.AppendLine("            _mapper = mapper;");
            sb.AppendLine("        }");
            sb.AppendLine();

            // Create method
            sb.AppendLine($"        public async Task<{modelName}Dto> Create({modelName}Dto entity)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var newRecord = _mapper.Map<{modelName}>(entity);");
            sb.AppendLine("            await _Repository.InsertAsync(newRecord);");
            sb.AppendLine("            await _unitOfWork.SaveChangesAsync();");
            sb.AppendLine($"            return _mapper.Map<{modelName}Dto>(newRecord);");
            sb.AppendLine("        }");
            sb.AppendLine();

            // Delete method
            sb.AppendLine("        public async Task<bool> Delete(int id)");
            sb.AppendLine("        {");
            sb.AppendLine("            var currentRecord = await _Repository.GetByIdAsync(id);");
            sb.AppendLine("            if (currentRecord == null)");
            sb.AppendLine("            {");
            sb.AppendLine("                return false;");
            sb.AppendLine("            }");
            sb.AppendLine("            _Repository.Delete(currentRecord);");
            sb.AppendLine("            await _unitOfWork.SaveChangesAsync();");
            sb.AppendLine("            return true;");
            sb.AppendLine("        }");
            sb.AppendLine();

            // GetAll with pagination
            sb.AppendLine($"        public async Task<PaginatedResult<{modelName}Dto>> GetAll(int pageNumber, int pageSize)");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                var res = await _Repository.GetAllPagination(pageNumber, pageSize);");
            sb.AppendLine($"                var mappedData = _mapper.Map<List<{modelName}Dto>>(res.Data);");
            sb.AppendLine("                mappedData.Reverse();");
            sb.AppendLine();
            sb.AppendLine($"                return PaginatedResult<{modelName}Dto>.Success(");
            sb.AppendLine("                    mappedData,");
            sb.AppendLine("                    res.TotalCount,");
            sb.AppendLine("                    res.CurrentPage,");
            sb.AppendLine("                    res.PageSize");
            sb.AppendLine("                );");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine("                throw new Exception(\"Error retrieving paginated data.\", ex);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();

            // GetById method
            sb.AppendLine($"        public async Task<{modelName}Dto> GetById(int id)");
            sb.AppendLine("        {");
            sb.AppendLine("            var currentRecord = await _Repository.GetByIdAsync(id);");
            sb.AppendLine($"            return _mapper.Map<{modelName}Dto>(currentRecord);");
            sb.AppendLine("        }");
            sb.AppendLine();

            // Update method
            sb.AppendLine($"        public async Task<{modelName}Dto> Update({modelName}Dto entity)");
            sb.AppendLine("        {");
            sb.AppendLine("            var currentRecord = await _Repository.GetByIdAsync(entity.Id);");
            sb.AppendLine("            if (currentRecord == null)");
            sb.AppendLine($"                throw new Exception(\"{modelName} not found\");");
            sb.AppendLine();
            sb.AppendLine("            _mapper.Map(entity, currentRecord);");
            sb.AppendLine("            _Repository.Update(currentRecord);");
            sb.AppendLine("            await _unitOfWork.SaveChangesAsync();");
            sb.AppendLine($"            return _mapper.Map<{modelName}Dto>(currentRecord);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            await FileWriterGenerator.WriteToFileAsync(Path.Combine(outputPath, $"{modelName}Service.cs"), sb.ToString());
        }



    }
}
