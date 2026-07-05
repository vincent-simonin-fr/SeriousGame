using Client;
using Client.Options;
using Client.Resources;
using Client.Services;
using Client.Services.Interfaces;
using Client.State;
using Client.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal class Program
{
    static async Task Main(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>();

        var configuration = configurationBuilder.Build();

        var services = new ServiceCollection();
        services.Configure<WebSocketServerOptions>(configuration.GetSection("WebSocketServer"));

        // Passer en Debug pour un débogage approfondi
        // Passer en Information en production
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddScoped<ClientSession>();
        services.AddScoped<ILobbyServices, LobbyServices>();
        services.AddScoped<IGameServices, GameServices>();
        services.AddScoped<App>();

        using var serviceProvider = services.BuildServiceProvider();

        // Un seul scope explicite pour toute la durée de l'exécution du client -
        // le Client n'a pas d'équivalent au scope-par-appel-de-hub du Server, donc on le crée nous-mêmes.
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();

        Console.Title = ClientResources.WindowTitle;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Clear();

        ConsoleUI.WriteHeader(ClientResources.GameName);

        var app = scope.ServiceProvider.GetRequiredService<App>();
        await app.RunAsync();

        var session = scope.ServiceProvider.GetRequiredService<ClientSession>();
        ConsoleUI.WriteInfo(string.Format(ClientResources.GoodbyeMessage, session.Nickname ?? ClientResources.NotLoggedInLabel));
    }
}