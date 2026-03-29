var builder = DistributedApplication.CreateBuilder(args);

var server = builder.AddProject<Projects.BirthdayCollator_Server>("server")
                    .WithExternalHttpEndpoints();

builder.AddViteApp("frontend", "../BirthdayCollator.Client")
       .WithReference(server)
       .WithExternalHttpEndpoints();

builder.Build().Run();