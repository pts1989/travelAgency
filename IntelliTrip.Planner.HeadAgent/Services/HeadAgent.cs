using IntelliTrip.Planner.Agents.Flight.Services;
using IntelliTrip.Planner.Agents.Shared.Interfaces;
using IntelliTrip.Planner.Models.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTrip.Planner.HeadAgent.Services;

public class HeadAgentAgent : IHeadAgent
{
    private readonly Kernel _kernel;
    private readonly IFlightAgent _flightAgent;

    public HeadAgentAgent(Kernel kernel, IFlightAgent flightAgent)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _flightAgent = flightAgent ?? throw new ArgumentNullException(nameof(flightAgent));
    }

    public async Task<ChatCompletionAgent> CreateHeadAgent()
    {
        ChatCompletionAgent triageAgent =
          new()
          {
              Name = "TriageAgent",
              Instructions = "A customer support agent that triages issues.",
              Kernel = _kernel,
              Description = "Handle customer requests.",
              Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
          };

        await using IMcpClient mcpClient = await CreateMcpClientAsync();


        // Create a kernel and register the MCP tools
        return triageAgent;
    }

    public async Task<HeadAgentResult> ProcessUserRequestAsync(HeadAgentRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        if (IsFlightQuery(request.UserMessage))
        {
            var query = ParseFlightQueryFromUserMessage(request.UserMessage, _flightAgent, cancellationToken).GetAwaiter().GetResult();
            var flightResults = await _flightAgent.SearchFlightsAsync(query, cancellationToken).ConfigureAwait(false);
            var responseText = string.Join("\n", flightResults.Select(f => $"{f.Airline} {f.FlightNumber}: {f.From}-{f.To} {f.DepartureTime:yyyy-MM-dd HH:mm} €{f.Price}"));
            return new HeadAgentResult { AgentResponse = responseText };
        }

        // Example: Use Semantic Kernel to get a response (replace with your actual prompt/logic)
        var result = await _kernel.InvokePromptAsync(
            "You are the HeadAgent for IntelliTrip. Answer the user's travel planning question: {{$input}}",
            new KernelArguments { ["input"] = request.UserMessage }
        ).ConfigureAwait(false);

        return new HeadAgentResult { AgentResponse = result.GetValue<string>() ?? string.Empty };
    }

    public async IAsyncEnumerable<string> StreamUserRequestAsync(HeadAgentRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        if (IsFlightQuery(request.UserMessage))
        {
            var query = ParseFlightQueryFromUserMessage(request.UserMessage, _flightAgent, cancellationToken).GetAwaiter().GetResult();
            var flightResults = await _flightAgent.SearchFlightsAsync(query, cancellationToken).ConfigureAwait(false);
            foreach (var f in flightResults)
            {
                yield return $"{f.Airline} {f.FlightNumber}: {f.From}-{f.To} {f.DepartureTime:yyyy-MM-dd HH:mm} €{f.Price}\n";
            }
            yield break;
        }

        // Example: Use Semantic Kernel's streaming API (adjust as needed for your LLM)
        await foreach (var update in _kernel.InvokePromptStreamingAsync(
            "You are the HeadAgent for IntelliTrip. Answer the user's travel planning question: {{$input}}",
            new KernelArguments { ["input"] = request.UserMessage },
            cancellationToken: cancellationToken))
        {
            var text = update.ToString();
            if (!string.IsNullOrEmpty(text))
                yield return text;
        }
    }

    protected static Task<IMcpClient> CreateMcpClientAsync(
        Kernel? kernel = null,
        Func<Kernel, CreateMessageRequestParams?, IProgress<ProgressNotificationValue>, CancellationToken, Task<CreateMessageResult>>? samplingRequestHandler = null)
    {
        KernelFunction? skSamplingHandler = null;

        // Create and return the MCP client
        return McpClientFactory.CreateAsync(
            clientTransport: new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "MCPServer",
                Command = GetMCPServerPath(), // Path to the MCPServer executable
            }),
            clientOptions: samplingRequestHandler != null ? new McpClientOptions()
            {
                Capabilities = new ClientCapabilities
                {
                    Sampling = new SamplingCapability
                    {
                        SamplingHandler = InvokeHandlerAsync
                    },
                },
            } : null
         );

        async ValueTask<CreateMessageResult> InvokeHandlerAsync(CreateMessageRequestParams? request, IProgress<ProgressNotificationValue> progress, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            skSamplingHandler ??= KernelFunctionFactory.CreateFromMethod(
                (CreateMessageRequestParams? request, IProgress<ProgressNotificationValue> progress, CancellationToken ct) =>
                {
                    return samplingRequestHandler(kernel!, request, progress, ct);
                },
                "MCPSamplingHandler"
            );

            // The argument names must match the parameter names of the delegate the SK Function is created from
            KernelArguments kernelArguments = new()
            {
                ["request"] = request,
                ["progress"] = progress
            };

            FunctionResult functionResult = await skSamplingHandler.InvokeAsync(kernel!, kernelArguments, cancellationToken);

            return functionResult.GetValue<CreateMessageResult>()!;
        }
    }

    private static string GetMCPServerPath()
    {
        // Determine the configuration (Debug or Release)  
        string configuration;

#if DEBUG
        configuration = "Debug";
#else
        configuration = "Release";
#endif

        return Path.Combine("..", "..", "..", "..", "MCPServer", "bin", configuration, "net8.0", "MCPServer.exe");
    }

    private static bool IsFlightQuery(string userMessage)
    {
        // Simple keyword-based detection; replace with NLP/classifier as needed
        var keywords = new[] { "vlucht", "flight", "vliegen", "airline", "ticket", "luchthaven", "aankomst", "vertrek" };
        return keywords.Any(k => userMessage.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<FlightQuery> ParseFlightQueryFromUserMessage(string userMessage, IFlightAgent flightAgent, CancellationToken cancellationToken)
    {
        // Example: "vlucht van Amsterdam naar New York op 2025-06-10 voor 2 personen"
        string? from = null, to = null;
        var date = DateTime.UtcNow.AddDays(7);
        var passengers = 1;
        var tokens = userMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < tokens.Length; i++)
        {
            if (tokens[i].Equals("van", StringComparison.OrdinalIgnoreCase) && i + 1 < tokens.Length)
                from = tokens[i + 1];
            if (tokens[i].Equals("naar", StringComparison.OrdinalIgnoreCase) && i + 1 < tokens.Length)
                to = tokens[i + 1];
            if (tokens[i].Equals("op", StringComparison.OrdinalIgnoreCase) && i + 1 < tokens.Length && DateTime.TryParse(tokens[i + 1], out var parsedDate))
                date = parsedDate;
            if ((tokens[i].Equals("voor", StringComparison.OrdinalIgnoreCase) || tokens[i].Equals("for", StringComparison.OrdinalIgnoreCase)) && i + 1 < tokens.Length && int.TryParse(tokens[i + 1], out var parsedPax))
                passengers = parsedPax;
        }
        // Use FlightAgent to resolve city/airport names to IATA codes
        if (!string.IsNullOrWhiteSpace(from))
            from = await flightAgent.ResolveAirportCodeAsync(from, cancellationToken) ?? from;
        if (!string.IsNullOrWhiteSpace(to))
            to = await flightAgent.ResolveAirportCodeAsync(to, cancellationToken) ?? to;
        return new FlightQuery
        {
            From = from ?? "" ,
            To = to ?? "",
            DepartureDate = date,
            Passengers = passengers
        };
    }
}
