       //services.AddSingleton<AutoCodeGenerator>(provider =>
       //    new AutoCodeGenerator(
       //        provider.GetRequiredService<IHostEnvironment>(),
       //        provider.GetRequiredService<ILogger<AutoCodeGenerator>>(),
       //        "Seagull.FrameWork.Repository.ModelCodeGenerator"));
       //services.AddHostedService<CodeGenerationHostedService>();


           [AutoGenerate("Seagull.FrameWork.Repository.ModelCodeGenerator.Test",
GenerateConfig = true,
GenerateDto = true,
GenerateMapper = true,
GenerateRepository = true,
GenerateService = true)]
