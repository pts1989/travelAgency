namespace IntelliTrip.Planner.Models.Models;

public sealed class HeadAgentRequest
{
    public required string UserMessage { get; init; }
}

public sealed class HeadAgentResult
{
    public required string AgentResponse { get; init; }
}
