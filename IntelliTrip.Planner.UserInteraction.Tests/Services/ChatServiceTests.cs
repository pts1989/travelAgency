using IntelliTrip.Planner.UserInteraction.Models;
using IntelliTrip.Planner.UserInteraction.Services;
using Xunit;
using System.Threading.Tasks;
using Moq;
using IntelliTrip.Planner.HeadAgent.Services;
using IntelliTrip.Planner.HeadAgent.Models;

namespace IntelliTrip.Planner.UserInteraction.Tests.Services;

public class ChatServiceTests
{
    [Fact]
    public async Task GetAgentResponseAsync_ReturnsEchoResponse()
    {
        // Arrange
        var headAgentMock = new Mock<IHeadAgent>();
        headAgentMock.Setup(h => h.ProcessUserRequestAsync(
                It.IsAny<HeadAgentRequest>(),
                default))
            .ReturnsAsync(new HeadAgentResult { AgentResponse = "Echo: Testbericht" });
        var service = new ChatService(headAgentMock.Object);
        var request = new ChatMessageRequest { Message = "Testbericht" };

        // Act
        var response = await service.GetAgentResponseAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Contains("Testbericht", response.Response);
    }
}
