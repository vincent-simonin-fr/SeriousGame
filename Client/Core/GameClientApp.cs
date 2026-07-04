using Client.Services;
using Client.UI;
using Microsoft.Extensions.Logging;

namespace Client.Core;

public class GameClientApp
{
    private readonly ILogger<GameClientApp> _logger;
    private readonly LobbyServices _lobbyServices;

    public GameClientApp(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GameClientApp>();
        _lobbyServices = new LobbyServices("https://localhost:7289/lobby");
    }

    public async Task RunAsync()
    {
        _logger.LogDebug("Your client ID: {clientId}", ClientIdentity.Id);

        var isSuccessfullyConnected = await _lobbyServices.ConnectAsync();

        if (!isSuccessfullyConnected) return;
        
        ConsoleUI.WritePrompt("Choose your login:");
        var login = ConsoleUI.ReadPrompt() ?? $"Player_{Guid.NewGuid().ToString()[..6]}";
        
        // Identification côté serveur
        await _lobbyServices.IdentifyClientAsync(login);

        _logger.LogDebug("✅ Connected to the server!.");

        await MainMenuLoop();
    }
    
    private async Task MainMenuLoop()
    {
        var hasMadeValidChoice = false;
        
        while (!hasMadeValidChoice)
        {
            ConsoleUI.WriteHeader("Main menu");
            ConsoleUI.WritePrompt("1. Create a game");
            ConsoleUI.WritePrompt("2. Join a game");
            ConsoleUI.WritePrompt("3. Quit");

            ConsoleUI.WritePrompt("\nYour choice:");
            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    await _lobbyServices.CreateGameAsync();
                    hasMadeValidChoice = true;
                    break;
                case "2":
                    await _lobbyServices.DisplayAndJoinGameAsync();
                    hasMadeValidChoice = true;
                    break;
                case "3":
                    await _lobbyServices.DisconnectAsync();
                    return;
                default:
                    ConsoleUI.WriteError("Your choice isn't valid.");
                    break;
            }
        }
    }

    private async Task InteractionLoop()
    {
        while (true)
        {
            ConsoleUI.WritePrompt("Type a message (‘exit’ to quit):");
            var input = ConsoleUI.ReadPrompt();

            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
                break;

            if (!string.IsNullOrWhiteSpace(input))
                await _lobbyServices.SendAsync("SendMessageToAllPlayers", input);
        }

        await _lobbyServices.DisconnectAsync();
    }
}
