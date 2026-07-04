using Client;
using Client.Core;
using Client.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

internal class Program
{
    static async Task Main(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        
        var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>();

        var configuration = builder.Build();
        
        // Set level to Debug for deep debugging
        // Set to information in production
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        Console.Title = @"🎮 Why So Serious!";
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Clear();
        
        ConsoleUI.WriteHeader(ClientResources.GameName);

        var app = new GameClientApp(loggerFactory);
        await app.RunAsync();

        ConsoleUI.WriteInfo($"Good bye {ClientIdentity.Nickname}! See you soon!.");
    }
}