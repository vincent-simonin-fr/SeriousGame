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

        RegisterHandlers();
    }

    private void RegisterHandlers()
    {
        var waitingAnim = new ConsoleAnimator(ClientResources.WaitingAnimationLabel, Bounce, 200);

        _lobbyConnection.On<List<GameDto>>(nameof(ILobbyHubClient.GamesUpdated), games =>
        {
            ClientMemory.Games = games;
        });

        _lobbyConnection.On<GameDto, List<string>>(nameof(ILobbyHubClient.WaitingForPlayers), (game, playerNames) =>
        {
            ConsoleUI.WriteHeader(string.Format(ClientResources.WaitingForGameHeader, game.Name));
            ConsoleUI.WritePrompt(string.Format(ClientResources.PlayersWaitingPrompt, playerNames.Count, game.MinimumPlayers, string.Join(", ", playerNames)));
            waitingAnim.Start();
        });

        _lobbyConnection.On<GameDto>(nameof(ILobbyHubClient.GameStarting), game =>
        {
            waitingAnim.Stop();
            ConsoleUI.WriteInfo(string.Format(ClientResources.GameStartingMessage, game.Name));
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

    public async Task CreateGameAsync()
    {
        ConsoleUI.WritePrompt(ClientResources.ChooseGameNamePrompt);
        var gameName = ConsoleUI.ReadPrompt() ?? $"Game_{Guid.NewGuid().ToString()[..6]}";

        var command = new CreateGameCommand { PlayerId = ClientIdentity.Id, GameName = gameName };
        ClientMemory.CurrentGame = await _lobbyConnection.InvokeAsync<GameDto>(nameof(ILobbyHubServer.CreateGame), command);

        ConsoleUI.WriteInfo(string.Format(ClientResources.GameCreatedMessage, gameName));

        await StartGameAsync();
    }

    public async Task DisplayAndJoinGameAsync()
    {
        if (ClientMemory.Games.Count == 0)
        {
            ConsoleUI.WriteInfo(ClientResources.NoGamesAvailableMessage);
            return;
        }

        ConsoleUI.WriteInfo(ClientResources.GamesAvailableHeader);
        for (int i = 0; i < ClientMemory.Games.Count; i++)
            ConsoleUI.WritePrompt(string.Format(ClientResources.GameListItemFormat, i + 1, ClientMemory.Games[i].Name, ClientMemory.Games[i].Players.Count, ClientMemory.Games[i].MinimumPlayers));

        // TODO : Permettre au joueur de revenir au menu principal
        ConsoleUI.WritePrompt(string.Format(ClientResources.ReturnToMainMenuFormat, ClientMemory.Games.Count + 1));

        ConsoleUI.WritePrompt(ClientResources.EnterGameNumberPrompt);

        if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= ClientMemory.Games.Count)
        {
            var selectedGame = ClientMemory.Games[choice - 1];
            var command = new JoinGameCommand { PlayerId = ClientIdentity.Id, GameId = selectedGame.Id };
            ClientMemory.CurrentGame = await _lobbyConnection.InvokeAsync<GameDto?>(nameof(ILobbyHubServer.JoinGame), command);

            if (ClientMemory.CurrentGame is null)
            {
                ConsoleUI.WriteError(string.Format(ClientResources.CannotJoinGameError, selectedGame.Name));
                await DisplayAndJoinGameAsync();
            }

            ConsoleUI.WriteInfo(string.Format(ClientResources.JoinedGameMessage, selectedGame.Name));

            await StartGameAsync();
        }
        else
        {
            ConsoleUI.WriteError(ClientResources.InvalidChoiceMessage);
            await DisplayAndJoinGameAsync();
        }
    }

    private async Task CreatePlayerCompanyAsync()
    {
        ConsoleUI.WriteHeader(ClientResources.CreateCompanyHeader);
        ConsoleUI.WritePrompt(ClientResources.CompanyNamePrompt);
        var companyName = ConsoleUI.ReadPrompt();

    }

    private async Task StartGameAsync()
    {
        // TODO : Améliorer cette vérification de null
        if (ClientMemory.CurrentGame is null) return;

        while (ClientMemory.CurrentGame.Players.Count < ClientMemory.CurrentGame.MinimumPlayers)
        {

        }

        // Le déroulement des tours n'est pas encore implémenté - GameDto omet volontairement Rounds (voir docs/architecture.md).

        await Task.CompletedTask;
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
