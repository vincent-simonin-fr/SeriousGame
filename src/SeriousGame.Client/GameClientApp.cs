using Client.Options;
using Client.Resources;
using Client.Services;
using Client.State;
using Client.UI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared;

namespace Client;

public class GameClientApp
{
    private readonly ILogger<GameClientApp> _logger;
    private readonly LobbyServices _lobbyServices;

    public GameClientApp(ILoggerFactory loggerFactory, IOptions<WebSocketServerOptions> webSocketServerOptions)
    {
        _logger = loggerFactory.CreateLogger<GameClientApp>();
        var options = webSocketServerOptions.Value;
        var hubUrl = $"{options.Scheme}://{options.Domain}:{options.Port}{HubRoutes.Lobby}";
        _lobbyServices = new LobbyServices(hubUrl);
    }

    public async Task RunAsync()
    {
        _logger.LogDebug("Your client ID: {clientId}", ClientIdentity.Id);

        var isSuccessfullyConnected = await _lobbyServices.ConnectAsync();

        if (!isSuccessfullyConnected) return;

        ConsoleUI.WritePrompt(ClientResources.ChooseLoginPrompt);
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
            ConsoleUI.WriteHeader(ClientResources.MainMenuHeader);
            ConsoleUI.WritePrompt(ClientResources.MenuOptionCreateGame);
            ConsoleUI.WritePrompt(ClientResources.MenuOptionJoinGame);
            ConsoleUI.WritePrompt(ClientResources.MenuOptionQuit);

            ConsoleUI.WritePrompt(ClientResources.YourChoicePrompt);
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
                    ConsoleUI.WriteError(ClientResources.InvalidChoiceError);
                    break;
            }
        }
    }

    private async Task InteractionLoop()
    {
        while (true)
        {
            ConsoleUI.WritePrompt(ClientResources.ExitPrompt);
            var input = ConsoleUI.ReadPrompt();

            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
                break;

            if (!string.IsNullOrWhiteSpace(input))
                await _lobbyServices.SendAsync(Constants.SendMessageToAllPlayersMethod, input);
        }

        await _lobbyServices.DisconnectAsync();
    }
}
