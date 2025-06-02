using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using IntelliTrip.Planner.Frontend.Blazor;
using IntelliTrip.Planner.Frontend.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Set the correct backend API base address for local development
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7094/") });
builder.Services.AddSingleton(new ChatSignalRService("https://localhost:7094/chathub"));

await builder.Build().RunAsync();
