var builder = DistributedApplication.CreateBuilder(args);

// Add the UserInteraction API as a resource in Aspire (use valid resource names)
var userInteractionApi = builder.AddProject("userinteraction-api", "../IntelliTrip.Planner.UserInteraction/IntelliTrip.Planner.UserInteraction.csproj");
userInteractionApi.WithEndpoint(name: "api-http", port: 5200, scheme: "http");

// Add the Blazor frontend as a resource in Aspire (use valid resource names)
var blazorFrontend = builder.AddProject("frontend-blazor", "../IntelliTrip.Planner.Frontend.Blazor/IntelliTrip.Planner.Frontend.Blazor.csproj");
blazorFrontend.WithEndpoint(name: "web-http", port: 5100, scheme: "http");
blazorFrontend.WithReference(userInteractionApi);

builder.Build().Run();
