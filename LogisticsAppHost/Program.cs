var builder = DistributedApplication.CreateBuilder(args);

var logisticsApi = builder.AddProject<Projects.logistic_web_api>("logistics-api");
builder.AddProject<Projects.LogisticsWebApp>("logistics-webapp")
    .WithReference(logisticsApi);

builder.Build().Run();
                            