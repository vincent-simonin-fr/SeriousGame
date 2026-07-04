using Client.Resources;
using Client.Services;
using Client.Services.Interfaces;
using Client.State;
using Client.UI;
using Microsoft.Extensions.Logging;

namespace Client;

public class App
{
    private readonly ILogger<App> _logger;
    private readonly ILobbyServices _lobbyServices;

    public App(ILogger<App> logger, ILobbyServices lobbyServices)
    {
        _logger = logger;
        _lobbyServices = lobbyServices;
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
        // Boucle d'enrôlement : chaque issue (partie jouée, échec, retour) ramène au menu,
        // seule l'option Quitter sort de la boucle.
        while (true)
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
                    await HandleEnrollmentAsync(await _lobbyServices.CreateGameAsync());
                    break;
                case "2":
                    await HandleEnrollmentAsync(await _lobbyServices.JoinGameAsync());
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

    private async Task HandleEnrollmentAsync(EnrollmentResult result)
    {
        switch (result)
        {
            case EnrollmentResult.WaitingForPlayers:
                var started = await _lobbyServices.WaitForGameStartAsync();
                if (!started)
                {
                    // Le joueur a annulé l'attente (Échap) : quitter proprement la partie côté serveur.
                    await _lobbyServices.LeaveGameAsync();
                }
                // Démarrée ou annulée : retour au menu (la boucle de partie n'existe pas encore).
                break;
            case EnrollmentResult.GameStarting:
                // Point d'entrée de la future boucle de partie.
                break;
            // Failed / NoGamesAvailable / ReturnToMenu : retour direct au menu.
        }
    }

    // Non branchée : boucle de chat conservée pour référence, jamais appelée.
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
