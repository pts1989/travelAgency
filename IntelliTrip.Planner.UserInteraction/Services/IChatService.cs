namespace IntelliTrip.Planner.UserInteraction.Services;

using IntelliTrip.Planner.UserInteraction.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

public interface IChatService
{
    Task GetAgentResponseAsync(ChatMessageRequest request);
    IAsyncEnumerable<string> StreamAgentResponseAsync(ChatMessageRequest request, CancellationToken cancellationToken = default);
}
