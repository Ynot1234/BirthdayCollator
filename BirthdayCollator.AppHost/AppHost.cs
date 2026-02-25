var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BirthdayCollator_Server>("birthdaycollator-server");




builder.Build().Run();
