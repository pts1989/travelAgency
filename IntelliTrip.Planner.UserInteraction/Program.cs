using IntelliTrip.Planner.HeadAgent; // Needed for AddHeadAgent extension
using IntelliTrip.Planner.UserInteraction.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Register HeadAgent and Semantic Kernel
builder.Services.AddHeadAgent(builder.Configuration);
builder.Services.AddScoped<IntelliTrip.Planner.UserInteraction.Services.IChatService, IntelliTrip.Planner.UserInteraction.Services.ChatService>();
builder.Services.AddScoped<IntelliTrip.Planner.UserInteraction.Services.ChatService>();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true); // For dev: allow all origins. Restrict in production!
    });
});
// Add Swagger/OpenAPI support for API testing and documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(); // CORS must be before UseAuthorization and endpoints
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
