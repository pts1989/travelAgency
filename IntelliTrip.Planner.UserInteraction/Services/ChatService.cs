using IntelliTrip.Planner.Agents.Shared.Interfaces;
using IntelliTrip.Planner.HeadAgent.Services;
using IntelliTrip.Planner.Models.Models;
using IntelliTrip.Planner.UserInteraction.Models;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Handoff;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.AspNetCore.SignalR;

namespace IntelliTrip.Planner.UserInteraction.Services
{ 

    public class ChatService : IChatService
    {
        private readonly IHeadAgent _headAgent;
        private readonly Kernel _kernel;
        private readonly IHubContext<IntelliTrip.Planner.UserInteraction.Hubs.ChatHub> _hubContext;

        public ChatService(IHeadAgent headAgent, Kernel kernel, IHubContext<IntelliTrip.Planner.UserInteraction.Hubs.ChatHub> hubContext)
        {
            _headAgent = headAgent;
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public async Task GetAgentResponseAsync(ChatMessageRequest request)
        {
            ChatCompletionAgent triageAgent =
              new()
              {
                  Name = "TriageAgent",
                  Instructions = "A customer support agent that triages issues.",
                  Kernel = _kernel,
                  Description = "Handle customer requests.",
                  Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
              };

            ChatCompletionAgent statusAgent =
                new()
                {
                    Name = "OrderStatusAgent",
                    Instructions = "Handle order status requests.",
                    Description = "A customer support agent that checks order status."
                };
            statusAgent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(new OrderStatusPlugin()));
            ChatCompletionAgent returnAgent =
                new()
                {
                    Name= "OrderReturnAgent",
                    Instructions= "Handle order return requests.",
                    Description= "A customer support agent that handles order returns."
                };
            returnAgent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(new OrderReturnPlugin()));
            ChatCompletionAgent refundAgent =
                new()
                {
                    Name = "OrderRefundAgent",
                    Instructions = "Handle order refund requests.",
                    Description = "A customer support agent that handles order refund."
                };
            refundAgent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(new OrderRefundPlugin()));

            // Create a monitor to capturing agent responses (via ResponseCallback)
            // to display at the end of this sample. (optional)
            // NOTE: Create your own callback to capture responses in your application or service.
            OrchestrationMonitor monitor = new();
            // Define user responses for InteractiveCallback (since sample is not interactive)

            // Define the orchestration
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            HandoffOrchestration orchestration =
                new(OrchestrationHandoffs
                        .StartWith(triageAgent)
                        .Add(triageAgent, statusAgent, returnAgent, refundAgent)
                        .Add(statusAgent, triageAgent, "Transfer to this agent if the issue is not status related")
                        .Add(returnAgent, triageAgent, "Transfer to this agent if the issue is not return related")
                        .Add(refundAgent, triageAgent, "Transfer to this agent if the issue is not refund related"),
                    triageAgent,
                    statusAgent,
                    returnAgent,
                    refundAgent)
                {
                    ResponseCallback = monitor.ResponseCallback,
                };
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            // Start the runtime
            InProcessRuntime runtime = new();
            await runtime.StartAsync();


#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            OrchestrationResult<string> result = await orchestration.InvokeAsync(request.Message, runtime);
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            string text = await result.GetValueAsync(TimeSpan.FromSeconds(300));
            await _hubContext.Clients.All.SendAsync("BroadcastMessage", text);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", text);
            await runtime.RunUntilIdleAsync();

            //Console.WriteLine("\n\nORCHESTRATION HISTORY");
            //foreach (ChatMessageContent message in monitor.History)
            //{
            //    this.WriteAgentChatMessage(message);
            //}
        }



        // Use HeadAgent for real response
        //var headAgentResult = await _headAgent.ProcessUserRequestAsync(
        //        new HeadAgentRequest { UserMessage = request.Message }
        //    ).ConfigureAwait(false);

        //    return new ChatMessageResponse
        //    {
        //        Response = headAgentResult.AgentResponse
        //    };
        //}

        public async IAsyncEnumerable<string> StreamAgentResponseAsync(ChatMessageRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var token in _headAgent.StreamUserRequestAsync(new HeadAgentRequest { UserMessage = request.Message }, cancellationToken))
            {
                yield return token;
            }
        }



    }
}
public sealed class OrderStatusPlugin
{
    [KernelFunction]
    public string CheckOrderStatus(string orderId) => $"Order {orderId} is shipped and will arrive in 2-3 days.";
}

public sealed class OrderReturnPlugin
{
    [KernelFunction]
    public string ProcessReturn(string orderId, string reason) => $"Return for order {orderId} has been processed successfully.";
}

public sealed class OrderRefundPlugin
{
    [KernelFunction]
    public string ProcessReturn(string orderId, string reason) => $"Refund for order {orderId} has been processed successfully.";
}

public sealed class OrchestrationMonitor
{
    public ChatHistory History { get; } = [];

    public ValueTask ResponseCallback(ChatMessageContent response)
    {
        this.History.Add(response);
        return ValueTask.CompletedTask;
    }
}