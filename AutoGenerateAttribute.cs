using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Seagull.FrameWork.Repository.IRepositories.Shared;
using Seagull.FrameWork.Repository.Models.Award;
using Seagull.FrameWork.Repository.DTO.CyberSecurity;
using Seagull.FrameWork.Repository.Wrapper;

namespace Seagull.FrameWork.Repository.ModelCodeGenerator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoGenerateAttribute : Attribute
    {
        public string Namespace { get; set; }
        public bool GenerateConfig { get; set; } = true;
        public bool GenerateDto { get; set; } = true;
        public bool GenerateMapper { get; set; } = true;
        public bool GenerateRepository { get; set; } = true;
        public bool GenerateService { get; set; } = true;

        public AutoGenerateAttribute(string nameSpace = null)
        {
            Namespace = nameSpace;
        }
    }

    public class CodeGenerationHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CodeGenerationHostedService> _logger;

        public CodeGenerationHostedService(
            IServiceProvider serviceProvider,
            ILogger<CodeGenerationHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting automatic code generation...");
            try
            {
                var generator = _serviceProvider.GetRequiredService<AutoCodeGenerator>();
                await generator.GenerateAllMarkedClassesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Code generation failed");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class AutoCodeGenerator
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<AutoCodeGenerator> _logger;
        private readonly string _baseNamespace;

        public AutoCodeGenerator(
            IHostEnvironment hostEnvironment,
            ILogger<AutoCodeGenerator> logger,
            string baseNamespace = "YourProject")
        {
            _hostEnvironment = hostEnvironment;
            _logger = logger;
            _baseNamespace = baseNamespace;
        }

        public async Task GenerateAllMarkedClassesAsync()
        {
            _logger.LogInformation("Scanning for classes with [AutoGenerate] attribute...");

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .ToList();

            var loadedAssemblies = new HashSet<Assembly>(assemblies);
            var queue = new Queue<Assembly>(assemblies);

            while (queue.Count > 0)
            {
                var assembly = queue.Dequeue();
                foreach (var reference in assembly.GetReferencedAssemblies())
                {
                    if (loadedAssemblies.Any(a => a.FullName == reference.FullName)) continue;

                    try
                    {
                        var loadedAssembly = Assembly.Load(reference);
                        loadedAssemblies.Add(loadedAssembly);
                        queue.Enqueue(loadedAssembly);
                        _logger.LogDebug($"Loaded assembly: {reference.FullName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Could not load assembly: {reference.FullName}");
                    }
                }
            }

            var generatedCount = 0;

            foreach (var assembly in loadedAssemblies)
            {
                try
                {
                    _logger.LogDebug($"Scanning assembly: {assembly.FullName}");
                    var markedTypes = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<AutoGenerateAttribute>() != null)
                        .ToList();

                    foreach (var type in markedTypes)
                    {
                        var attribute = type.GetCustomAttribute<AutoGenerateAttribute>();
                        _logger.LogInformation($"Found [AutoGenerate] on: {type.FullName}");

                        var nameSpace = attribute.Namespace ?? _baseNamespace;
                        await GenerateAllFilesForTypeAsync(type, attribute, nameSpace);
                        generatedCount++;
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    _logger.LogWarning($"Could not load types from assembly: {assembly.FullName}. Loader exceptions: {string.Join(", ", ex.LoaderExceptions.Select(e => e.Message))}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing assembly: {assembly.FullName}");
                }
            }

            _logger.LogInformation($"Code generation complete. Generated files for {generatedCount} classes.");
        }

        private async Task GenerateAllFilesForTypeAsync(Type modelType, AutoGenerateAttribute attribute, string nameSpace)
        {
            var modelName = modelType.Name;
            var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

            var baseOutputPath = Path.Combine(_hostEnvironment.ContentRootPath, "GeneratedCode");

            var directories = new Dictionary<string, string>
            {
                { "Config", Path.Combine(baseOutputPath, "Config") },
                { "Dto", Path.Combine(baseOutputPath, "DTOs") },
                { "Profile", Path.Combine(baseOutputPath, "Profiles") },
                { "IRepository", Path.Combine(baseOutputPath, "Repositories", "Interfaces") },
                { "Repository", Path.Combine(baseOutputPath, "Repositories", "Implementations") },
                { "IService", Path.Combine(baseOutputPath, "Services", "Interfaces") },
                { "Service", Path.Combine(baseOutputPath, "Services", "Implementations") }
            };

            foreach (var dir in directories.Values)
            {
                Directory.CreateDirectory(dir);
            }

            if (attribute.GenerateConfig)
            {
                await ConfigGeneator.GenerateConfigFileAsync(modelType, properties, nameSpace, directories["Config"]);
            }

            if (attribute.GenerateDto)
            {
                await DtoGenerator.GenerateDtoAsync(modelType, properties, nameSpace, directories["Dto"]);
            }

            if (attribute.GenerateMapper)
            {
                await MapperGenerator.GenerateMapperProfileAsync(modelType, nameSpace, directories["Profile"]);
            }

            if (attribute.GenerateRepository)
            {
                await IRepostioryGenerator.GenerateIRepositoryAsync(modelType, nameSpace, directories["IRepository"]);
                await RepostioryGenerator.GenerateRepositoryAsync(modelType, nameSpace, directories["Repository"]);
            }

            if (attribute.GenerateService)
            {
                await IServiceGenerate.GenerateIServiceAsync(modelType, nameSpace, directories["IService"]);
                await ServiceGenerate.GenerateServiceAsync(modelType, nameSpace, directories["Service"]);
            }
        }


    
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAutoCodeGeneration(
            this IServiceCollection services,
            string baseNamespace = "YourProject")
        {
            services.AddSingleton<AutoCodeGenerator>(provider =>
                new AutoCodeGenerator(
                    provider.GetRequiredService<IHostEnvironment>(),
                    provider.GetRequiredService<ILogger<AutoCodeGenerator>>(),
                    baseNamespace));

            services.AddHostedService<CodeGenerationHostedService>();
            return services;
        }
    }
}
