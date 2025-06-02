using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using IntelliTrip.Planner.Agents.Flight.Services;
using IntelliTrip.Planner.Agents.Shared.Interfaces;
using IntelliTrip.Planner.Models.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

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

    public ChatCompletionAgent CreateHeadAgent()
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
