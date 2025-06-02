using IntelliTrip.Planner.HeadAgent.Services;
using IntelliTrip.Planner.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IntelliTrip.Planner.HeadAgent.Tests.Services;

public class HeadAgentTests
{
    [Fact]
    public async Task ProcessUserRequestAsync_ReturnsAgentResponse()
    {
     
        // Arrange 
        var mockChatCompletion = new Mock<IChatCompletionService>();
        mockChatCompletion
            .Setup(x => x.GetChatMessageContentsAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings>(),
                It.IsAny<Kernel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ChatMessageContent(AuthorRole.Assistant, "Test response")]);

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(mockChatCompletion.Object);

        var kernel = kernelBuilder.Build();

        var agent = new HeadAgentAgent(kernel,null);
        var request = new HeadAgentRequest { UserMessage = "Where can I travel in June?" };

        // Act
        var result = await agent.ProcessUserRequestAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test response", result.AgentResponse);
    }


}
