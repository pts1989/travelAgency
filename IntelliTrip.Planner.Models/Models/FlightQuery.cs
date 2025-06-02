namespace IntelliTrip.Planner.Models.Models;

public class FlightQuery
{
    public required string From { get; init; }
    public required string To { get; init; }
    public DateTime DepartureDate { get; init; }
    public DateTime? ReturnDate { get; init; }
    public int Passengers { get; init; } = 1;
}
