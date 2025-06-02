using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using IntelliTrip.Planner.HeadAgent.Services;
using IntelliTrip.Planner.Agents.Flight.Services;
using System.Net.Http;
using IntelliTrip.Planner.Agents.Shared.Interfaces;

namespace IntelliTrip.Planner.HeadAgent;

public static class HeadAgentServiceCollectionExtensions
{
    public static IServiceCollection AddHeadAgent(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<Kernel>(sp =>
        {
            var builder = Kernel.CreateBuilder();
            // Configure for GitHub Copilot (OpenAI-compatible)
            builder.AddOpenAIChatCompletion(
                modelId: configuration["Copilot:Model"],
                apiKey: configuration["Copilot:ApiKey"],
                endpoint: new Uri(configuration["Copilot:Endpoint"])
            );
            return builder.Build();
        });
        // Register FlightAgent with HttpClient for MCP API
        services.AddHttpClient<IFlightAgent, FlightAgent>();
        services.AddSingleton<IHeadAgent>(sp =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            var flightAgent = sp.GetRequiredService<IFlightAgent>();
            return new HeadAgentAgent(kernel, flightAgent);
        });
        return services;
    }
}
