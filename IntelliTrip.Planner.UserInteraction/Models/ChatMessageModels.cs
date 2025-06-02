namespace IntelliTrip.Planner.UserInteraction.Models;

public class ChatMessageRequest
{
    public string Message { get; set; } = string.Empty;
}

public class ChatMessageResponse
{
    public string Response { get; set; } = string.Empty;
}
