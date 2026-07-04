using Server.Application.Abstractions;
using Server.Application.Services;
using Server.Hubs;
using Server.Infrastructure;
using Shared;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
// builder.Services.AddControllers();

builder.Services.AddSingleton<AppMemory>();
builder.Services.AddSingleton<IGameRepository, InMemoryGameRepository>();
builder.Services.AddSingleton<IPlayerRepository, InMemoryPlayerRepository>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<ILobbyFlowService, LobbyFlowService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
}

app.UseHttpsRedirection();

app.MapHub<LobbyHub>(HubRoutes.Lobby);
app.MapHub<GameHub>(HubRoutes.Game);

app.Run();
