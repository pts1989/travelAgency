
using IntelliTrip.Planner.Models.Models;

namespace IntelliTrip.Planner.Agents.Shared.Interfaces;

public interface IFlightAgent
{
    /// <summary>
    /// Looks up possible flights for the given query.
    /// </summary>
    Task<IReadOnlyList<FlightResult>> SearchFlightsAsync(FlightQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves an airport code (IATA) from a city or airport name using an LLM or web API.
    /// </summary>
    Task<string?> ResolveAirportCodeAsync(string location, CancellationToken cancellationToken = default);

}
