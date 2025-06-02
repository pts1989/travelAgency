using IntelliTrip.Planner.Models.Models;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTrip.Planner.Agents.Shared.Interfaces;

public interface IHeadAgent
{
    Task<HeadAgentResult> ProcessUserRequestAsync(HeadAgentRequest request, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> StreamUserRequestAsync(HeadAgentRequest request, CancellationToken cancellationToken = default);
}
