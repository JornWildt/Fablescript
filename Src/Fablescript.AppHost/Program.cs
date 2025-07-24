var builder = DistributedApplication.CreateBuilder(args);

var client = builder.AddProject<Projects.Fablescript>("fablescript-console");

builder.Build().Run();
