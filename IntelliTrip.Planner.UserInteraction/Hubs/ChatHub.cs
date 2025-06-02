using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using IntelliTrip.Planner.UserInteraction.Services; // Ensure the correct namespace for IChatService
using IntelliTrip.Planner.UserInteraction.Models; // Ensure the correct namespace for ChatMessageRequest

namespace IntelliTrip.Planner.UserInteraction.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string message)
    {
        var chatService = Context.GetHttpContext()?.RequestServices.GetService<IChatService>();
        if (chatService != null)
        {
            var chatRequest = new ChatMessageRequest { Message = message };
            await chatService.GetAgentResponseAsync(chatRequest);
            //await foreach (var chunk in chatService.StreamAgentResponseAsync(chatRequest, Context.ConnectionAborted))
            //{
            //    // Send to the caller
            //    await Clients.Caller.SendAsync("ReceiveMessage", chunk);
            //    // Broadcast to all other clients (except sender)
            //    await Clients.Others.SendAsync("BroadcastMessage", chunk);
            //}
        }
        else
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "[Error] Chat service not available.");
        }
    }

    // Example: broadcast a system message to all clients
    public async Task BroadcastSystemMessage(string systemMessage)
    {
        await Clients.All.SendAsync("BroadcastMessage", systemMessage);
    }
}
