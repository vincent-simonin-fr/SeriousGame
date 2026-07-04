using Client.Options;
using Client.Resources;
using Client.Services.Interfaces;
using Client.State;
using Client.UI;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared;
using Shared.Abstractions;
using Shared.Models.Dtos;

namespace Client.Services;

public class LobbyServices : ILobbyServices
{
    private readonly string _hubUrl;
    private readonly ILogger<LobbyServices> _logger;
    private HubConnection _lobbyConnection;
    private TaskCompletionSource? _gameStartingSignal;
    private readonly ConsoleAnimator _waitingAnim;
    public static string[] Dots => [".", "..", "...", "....", "....."];
    private static string[] Bounce => ["⬤     ", " ⬤    ", "  ⬤   ", "   ⬤  ", "    ⬤ ", "     ⬤", "    ⬤ ", "   ⬤  ", "  ⬤   ", " ⬤    "];

    public LobbyServices(IOptions<WebSocketServerOptions> webSocketServerOptions, ILogger<LobbyServices> logger)
    {
        _logger = logger;
        var options = webSocketServerOptions.Value;
        _hubUrl = $"{options.Scheme}://{options.Domain}:{options.Port}{HubRoutes.Lobby}";
        _lobbyConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .WithAutomaticReconnect()
            .Build();

        // Champ (et non variable locale de RegisterHandlers) : le chemin d'annulation (Échap)
        // doit pouvoir stopper l'animation, pas seulement le handler GameStarting.
        _waitingAnim = new ConsoleAnimator(ClientResources.WaitingAnimationLabel, Bounce, 200);

        RegisterHandlers();
    }

    private void RegisterHandlers()
    {
        _lobbyConnection.On<List<GameDto>>(nameof(ILobbyHubClient.GamesUpdated), games =>
        {
            ClientMemory.Games = games;
        });

        _lobbyConnection.On<GameDto, List<string>>(nameof(ILobbyHubClient.WaitingForPlayers), (game, playerNames) =>
        {
            ConsoleUI.WriteHeader(string.Format(ClientResources.WaitingForGameHeader, game.Name));
            ConsoleUI.WritePrompt(string.Format(ClientResources.PlayersWaitingPrompt, playerNames.Count, game.MinimumPlayers, string.Join(", ", playerNames)));
            _waitingAnim.Start();
        });

        _lobbyConnection.On<GameDto>(nameof(ILobbyHubClient.GameStarting), game =>
        {
            _waitingAnim.Stop();
            ConsoleUI.WriteInfo(string.Format(ClientResources.GameStartingMessage, game.Name));
            _gameStartingSignal?.TrySetResult();
        });

        _lobbyConnection.On<string>(nameof(ILobbyHubClient.Notify), msg =>
        {
            ConsoleUI.WriteInfo(string.Format(ClientResources.ServerMessagePrefix, msg));
        });

        _lobbyConnection.On<GameDto>(nameof(ILobbyHubClient.UpdateGameInProgressWhenPlayerQuits), game =>
        {
            if (ClientMemory.CurrentGame is null) return;
            ClientMemory.CurrentGame.Players.First(p => !game.Players.Select(player => player.Id).Contains(p.Id)).IsActive = false;
        });

        _lobbyConnection.Reconnecting += error =>
        {
            ConsoleUI.WriteError(ClientResources.ReconnectingWarning);
            return Task.CompletedTask;
        };

        _lobbyConnection.Reconnected += async id =>
        {
            await _lobbyConnection.InvokeAsync(nameof(ILobbyHubServer.UpdatePlayerConnectionId), ClientIdentity.Id);
            ConsoleUI.WriteInfo(ClientResources.ReconnectedMessage);
            await Task.CompletedTask;
        };
    }

    public async Task<bool> ConnectAsync()
    {
        try
        {
            await _lobbyConnection.StartAsync();
            ConsoleUI.WriteInfo(string.Format(ClientResources.ConnectedToHubMessage, _hubUrl));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Échec de connexion au hub {HubUrl}", _hubUrl);
            ConsoleUI.WriteError(string.Format(ClientResources.FailedToConnectError, _hubUrl));
            return false;
        }
    }

    public async Task IdentifyClientAsync(string nickname)
    {
        ClientIdentity.SetNickname(nickname);
        var command = new CreatePlayerCommand {  PlayerId = ClientIdentity.Id, Nickname = ClientIdentity.Nickname };
        await _lobbyConnection.InvokeAsync(nameof(ILobbyHubServer.IdentifyNewPlayer), command);
    }

    public async Task<EnrollmentResult> CreateGameAsync()
    {
        ConsoleUI.WritePrompt(ClientResources.ChooseGameNamePrompt);
        var gameName = ConsoleUI.ReadPrompt() ?? $"Game_{Guid.NewGuid().ToString()[..6]}";

        PrepareGameStartSignal();

        var command = new CreateGameCommand { PlayerId = ClientIdentity.Id, GameName = gameName };
        ClientMemory.CurrentGame = await _lobbyConnection.InvokeAsync<GameDto>(nameof(ILobbyHubServer.CreateGame), command);

        ConsoleUI.WriteInfo(string.Format(ClientResources.GameCreatedMessage, gameName));

        return ClientMemory.CurrentGame.IsInProgress
            ? EnrollmentResult.GameStarting
            : EnrollmentResult.WaitingForPlayers;
    }

    public async Task<EnrollmentResult> JoinGameAsync()
    {
        // Snapshot local : GamesUpdated peut remplacer la liste pendant la saisie.
        var games = ClientMemory.Games;

        if (games.Count == 0)
        {
            ConsoleUI.WriteInfo(ClientResources.NoGamesAvailableMessage);
            return EnrollmentResult.NoGamesAvailable;
        }

        ConsoleUI.WriteInfo(ClientResources.GamesAvailableHeader);
        for (int i = 0; i < games.Count; i++)
            ConsoleUI.WritePrompt(string.Format(ClientResources.GameListItemFormat, i + 1, games[i].Name, games[i].Players.Count, games[i].MinimumPlayers));

        var returnToMenuChoice = games.Count + 1;
        ConsoleUI.WritePrompt(string.Format(ClientResources.ReturnToMainMenuFormat, returnToMenuChoice));

        int choice;
        while (true)
        {
            ConsoleUI.WritePrompt(ClientResources.EnterGameNumberPrompt);
            if (int.TryParse(Console.ReadLine(), out choice) && choice > 0 && choice <= returnToMenuChoice)
                break;
            ConsoleUI.WriteError(ClientResources.InvalidChoiceMessage);
        }

        if (choice == returnToMenuChoice)
            return EnrollmentResult.ReturnToMenu;

        var selectedGame = games[choice - 1];

        PrepareGameStartSignal();

        var command = new JoinGameCommand { PlayerId = ClientIdentity.Id, GameId = selectedGame.Id };
        ClientMemory.CurrentGame = await _lobbyConnection.InvokeAsync<GameDto?>(nameof(ILobbyHubServer.JoinGame), command);

        if (ClientMemory.CurrentGame is null)
        {
            ConsoleUI.WriteError(string.Format(ClientResources.CannotJoinGameError, selectedGame.Name));
            return EnrollmentResult.Failed;
        }

        ConsoleUI.WriteInfo(string.Format(ClientResources.JoinedGameMessage, selectedGame.Name));

        return ClientMemory.CurrentGame.IsInProgress
            ? EnrollmentResult.GameStarting
            : EnrollmentResult.WaitingForPlayers;
    }

    private void PrepareGameStartSignal()
    {
        // Créé avant l'invoke hub : le handler GameStarting peut arriver sur un thread SignalR
        // avant que l'appelant n'atteigne le await - le signal doit déjà exister.
        _gameStartingSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public async Task<bool> WaitForGameStartAsync()
    {
        if (_gameStartingSignal is null) return true;

        // KeyAvailable lève une exception quand stdin est redirigé (exécution scriptée) :
        // dans ce cas la détection clavier est désactivée et on attend uniquement le signal.
        var canReadKeys = !Console.IsInputRedirected;
        if (canReadKeys) ConsoleUI.WritePrompt(ClientResources.CancelWaitingHint);

        while (!_gameStartingSignal.Task.IsCompleted)
        {
            if (canReadKeys && Console.KeyAvailable)
            {
                // intercept: true - la touche n'est pas affichée dans la console.
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Escape)
                {
                    _waitingAnim.Stop();
                    return false;
                }
                // Toute autre touche est consommée et ignorée (n'encombre pas la saisie du menu).
            }

            // Sondage léger : 10 vérifications/s, le thread dort entre deux.
            await Task.Delay(100);
        }

        // Le signal gagne sur Échap si les deux surviennent dans la même fenêtre.
        return true;
    }

    public async Task LeaveGameAsync()
    {
        if (ClientMemory.CurrentGame is null) return;

        var gameName = ClientMemory.CurrentGame.Name;
        await _lobbyConnection.InvokeAsync(nameof(ILobbyHubServer.LeaveGame));
        ClientMemory.CurrentGame = null;

        ConsoleUI.WriteInfo(string.Format(ClientResources.LeftGameMessageFormat, gameName));
    }

    private async Task CreatePlayerCompanyAsync()
    {
        ConsoleUI.WriteHeader(ClientResources.CreateCompanyHeader);
        ConsoleUI.WritePrompt(ClientResources.CompanyNamePrompt);
        var companyName = ConsoleUI.ReadPrompt();

    }

    public async Task SendAsync(string methodName, params object[] args)
    {
        if (_lobbyConnection.State != HubConnectionState.Connected)
        {
            ConsoleUI.WriteError(string.Format(ClientResources.CannotInvokeError, methodName, _lobbyConnection.State));
            return;
        }

        try
        {
            await _lobbyConnection.InvokeCoreAsync(methodName, args);
        }
        catch (HubException hex)
        {
            _logger.LogWarning(hex, "HubException lors de l'appel de '{MethodName}'", methodName);
            ConsoleUI.WriteError(string.Format(ClientResources.HubExceptionError, methodName, hex.Message));
        }
        catch (InvalidOperationException iox)
        {
            _logger.LogWarning(iox, "Opération invalide lors de l'appel de '{MethodName}'", methodName);
            ConsoleUI.WriteError(string.Format(ClientResources.InvalidOperationError, methodName, iox.Message));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur inattendue lors de l'appel de '{MethodName}'", methodName);
            ConsoleUI.WriteError(string.Format(ClientResources.UnexpectedError, methodName, ex.Message));
        }
    }

    public async Task<T?> SendAsync<T>(string methodName, params object[] args)
    {
        if (_lobbyConnection.State != HubConnectionState.Connected)
        {
            ConsoleUI.WriteError(string.Format(ClientResources.CannotInvokeError, methodName, _lobbyConnection.State));
            return default;
        }

        try
        {
            return await _lobbyConnection.InvokeCoreAsync<T>(methodName, args);
        }
        catch (HubException hex)
        {
            _logger.LogWarning(hex, "HubException lors de l'appel de '{MethodName}'", methodName);
            ConsoleUI.WriteError(string.Format(ClientResources.HubExceptionError, methodName, hex.Message));
        }
        catch (InvalidOperationException iox)
        {
            _logger.LogWarning(iox, "Opération invalide lors de l'appel de '{MethodName}'", methodName);
            ConsoleUI.WriteError(string.Format(ClientResources.InvalidOperationError, methodName, iox.Message));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur inattendue lors de l'appel de '{MethodName}'", methodName);
            ConsoleUI.WriteError(string.Format(ClientResources.UnexpectedError, methodName, ex.Message));
        }

        return default;
    }
    public async Task DisconnectAsync()
    {
        await _lobbyConnection.StopAsync();
        ConsoleUI.WriteInfo(ClientResources.DisconnectedFromLobbyMessage);
    }
}
