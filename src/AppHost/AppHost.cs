var builder = DistributedApplication.CreateBuilder(args);

var webApi = builder.AddProject<Projects.WebApi>("api");

var blazor = builder.AddProject<Projects.BlazorServer>("web");

builder.Build().Run();
