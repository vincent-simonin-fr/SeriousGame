using Server;
using Server.Hubs;
using Server.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
// builder.Services.AddControllers();

builder.Services.AddSingleton<AppMemory>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<PlayerService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
}

app.UseHttpsRedirection();

app.MapHub<LobbyHub>("/lobby");
app.MapHub<GameHub>("/game");

app.Run();
