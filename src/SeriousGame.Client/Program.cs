using Client;
using Client.Options;
using Client.Resources;
using Client.State;
using Client.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        using var serviceProvider = services.BuildServiceProvider();
        var webSocketServerOptions = serviceProvider.GetRequiredService<IOptions<WebSocketServerOptions>>();

        // Passer en Debug pour un débogage approfondi
        // Passer en Information en production
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        Console.Title = ClientResources.WindowTitle;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Clear();

        ConsoleUI.WriteHeader(ClientResources.GameName);

        var app = new GameClientApp(loggerFactory, webSocketServerOptions);
        await app.RunAsync();

        ConsoleUI.WriteInfo(string.Format(ClientResources.GoodbyeMessage, ClientIdentity.Nickname));
    }
}