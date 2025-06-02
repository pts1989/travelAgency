using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;

namespace IntelliTrip.Planner.Frontend.Blazor.Services;

public class ChatSignalRService
{
    private readonly HubConnection _connection;

    public event Func<string, Task>? OnMessageReceived;

    public ChatSignalRService(string hubUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string>("ReceiveMessage", async (message) =>
        {
            if (OnMessageReceived != null)
                await OnMessageReceived.Invoke(message);
        });
    }

    public async Task StartAsync() => await _connection.StartAsync();
    public async Task StopAsync() => await _connection.StopAsync();
    public async Task SendMessageAsync(string message) => await _connection.InvokeAsync("SendMessage", message);
}
