namespace IntelliTrip.Planner.Models.Models;

public class FlightResult
{
    public required string FlightNumber { get; init; }
    public required string Airline { get; init; }
    public required string From { get; init; }
    public required string To { get; init; }
    public DateTime DepartureTime { get; init; }
    public DateTime ArrivalTime { get; init; }
    public decimal Price { get; init; }
    public string? Currency { get; init; }
}
