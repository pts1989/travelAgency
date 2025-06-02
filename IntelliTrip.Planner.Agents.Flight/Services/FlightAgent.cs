using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using IntelliTrip.Planner.Agents.Shared.Interfaces;
using IntelliTrip.Planner.Models.Models;

namespace IntelliTrip.Planner.Agents.Flight.Services;

public class FlightAgent : IFlightAgent
{
    private readonly HttpClient _httpClient;
    private const string MCP_SEARCH_ENDPOINT = "https://api.apify.com/v2/actor-tasks/canadesk~google-flights/run-sync-get-dataset-items?token=YOUR_API_TOKEN";

    public FlightAgent(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IReadOnlyList<FlightResult>> SearchFlightsAsync(FlightQuery query, CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            from = query.From,
            to = query.To,
            departureDate = query.DepartureDate.ToString("yyyy-MM-dd"),
            returnDate = query.ReturnDate?.ToString("yyyy-MM-dd"),
            passengers = query.Passengers
        };
        try
        {
            var response = await _httpClient.PostAsJsonAsync(MCP_SEARCH_ENDPOINT, requestBody, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var mcpResults = await response.Content.ReadFromJsonAsync<List<McpFlightResult>>(cancellationToken: cancellationToken).ConfigureAwait(false);
            return mcpResults?.Select(MapMcpToFlightResult).ToList() ?? new List<FlightResult>();
        }
        catch (Exception ex)
        {
            throw new FlightAgentException("Failed to search flights via MCP API", ex);
        }
    }

    private static FlightResult MapMcpToFlightResult(McpFlightResult mcp)
    {
        return new FlightResult
        {
            FlightNumber = mcp.FlightNumber,
            Airline = mcp.Airline,
            From = mcp.From,
            To = mcp.To,
            DepartureTime = mcp.DepartureTime,
            ArrivalTime = mcp.ArrivalTime,
            Price = mcp.Price,
            Currency = mcp.Currency
        };
    }

    /// <summary>
    /// Resolves an airport code (IATA) from a city or airport name using an LLM or web API.
    /// </summary>
    public async Task<string?> ResolveAirportCodeAsync(string location, CancellationToken cancellationToken = default)
    {
        // Example: Call a public API for airport lookup (replace with LLM call if needed)
        // Here we use aviationstack as an example, but you can swap for any LLM or other API
        var apiKey = "YOUR_AVIATIONSTACK_API_KEY";
        var url = $"http://api.aviationstack.com/v1/airports?access_key={apiKey}&search={Uri.EscapeDataString(location)}";
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            // Simple extraction (replace with a proper model for production)
            var iata = ExtractIataFromJson(json);
            return iata;
        }
        catch (Exception ex)
        {
            // TODO: Add logging or fallback to LLM
            throw new FlightAgentException($"Failed to resolve airport code for '{location}'", ex);
        }
    }

    private static string? ExtractIataFromJson(string json)
    {
        // Very basic extraction for demo; use a proper JSON parser in production
        var iataIndex = json.IndexOf("\"iata_code\":");
        if (iataIndex >= 0)
        {
            var start = json.IndexOf('"', iataIndex + 12) + 1;
            var end = json.IndexOf('"', start);
            if (start > 0 && end > start)
                return json.Substring(start, end - start);
        }
        return null;
    }

    private class McpFlightResult
    {
        public string FlightNumber { get; set; } = string.Empty;
        public string Airline { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}

public class FlightAgentException : Exception
{
    public FlightAgentException(string message, Exception? inner = null) : base(message, inner) { }
}
