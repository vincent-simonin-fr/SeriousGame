using Client.Core;
using Client.UI;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Shared.Abstractions;
using Shared.Models.Dtos;

namespace Client.Services;

public class LobbyServices
{
    private readonly string _hubUrl;
    private HubConnection _lobbyConnection;
    public static string[] Dots => [".", "..", "...", "....", "....."];
    private static string[] Bounce => ["⬤     ", " ⬤    ", "  ⬤   ", "   ⬤  ", "    ⬤ ", "     ⬤", "    ⬤ ", "   ⬤  ", "  ⬤   ", " ⬤    "];

    public LobbyServices(string hubUrl)
    {
        _hubUrl = hubUrl;
        _lobbyConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .WithAutomaticReconnect()
            .Build();

        RegisterHandlers();
    }

    private void RegisterHandlers()
    {
        var waitingAnim = new ConsoleAnimator("Waiting for other players", Bounce, 200);
        
        _lobbyConnection.On<List<GameDto>>(nameof(ILobbyHubClient.GamesUpdated), games =>
        {
            ClientMemory.Games = games;
        });

        _lobbyConnection.On<GameDto, List<string>>(nameof(ILobbyHubClient.WaitingForPlayers), (game, playerNames) =>
        {
            ConsoleUI.WriteHeader($"🕹 Waiting for game {game.Name}");
            ConsoleUI.WritePrompt($"Players ({playerNames.Count}/{game.MinimumPlayers}) : {string.Join(", ", playerNames)}");
            waitingAnim.Start();
            //ConsoleUI.WritePrompt("Waiting for other players....");
        });

        _lobbyConnection.On<GameDto>(nameof(ILobbyHubClient.GameStarting), game =>
        {
            waitingAnim.Stop();
            ConsoleUI.WriteInfo($"\n🚀 The Game {game.Name} starts!");
        });

        _lobbyConnection.On<string>(nameof(ILobbyHubClient.Notify), msg =>
        {
            ConsoleUI.WriteInfo($"[From the Server] {msg}");
        });

        _lobbyConnection.On<GameDto>(nameof(ILobbyHubClient.UpdateGameInProgressWhenPlayerQuits), game =>
        {
            if (ClientMemory.CurrentGame is null) return;
            ClientMemory.CurrentGame.Players.First(p => !game.Players.Select(player => player.Id).Contains(p.Id)).IsActive = false;
        });

        _lobbyConnection.Reconnecting += error =>
        {
            ConsoleUI.WriteError("⚠️ Reconnecting...");
            return Task.CompletedTask;
        };

        _lobbyConnection.Reconnected += async id =>
        {
            await _lobbyConnection.InvokeAsync(nameof(ILobbyHubServer.UpdatePlayerConnectionId), ClientIdentity.Id);
            ConsoleUI.WriteInfo("🔗 Reconnected!");
            await Task.CompletedTask;
        };
    }

    public async Task<bool> ConnectAsync()
    {
        try
        {
            await _lobbyConnection.StartAsync();
            ConsoleUI.WriteInfo($"Connected to the Hub: {_hubUrl}");
            return true;
        }
        catch
        {
            ConsoleUI.WriteError($"Failed to connect to Hub: {_hubUrl}. " +
                                 $"If the problem persists, please contact an administrator.");
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
        ConsoleUI.WritePrompt(@"Choose a name for your game:");
        var gameName = ConsoleUI.ReadPrompt() ?? $"Game_{Guid.NewGuid().ToString()[..6]}";

        var command = new CreateGameCommand { PlayerId = ClientIdentity.Id, GameName = gameName };
        ClientMemory.CurrentGame = await _lobbyConnection.InvokeAsync<GameDto>(nameof(ILobbyHubServer.CreateGame), command);

        ConsoleUI.WriteInfo($"🧩 Game {gameName} created, waiting for other players...");

        await StartGameAsync();
    }

    public async Task DisplayAndJoinGameAsync()
    {
        if (ClientMemory.Games.Count == 0)
        {
            ConsoleUI.WriteInfo("No game available at this time.");
            return;
        }

        ConsoleUI.WriteInfo("\nGames available:\n");
        for (int i = 0; i < ClientMemory.Games.Count; i++)
            ConsoleUI.WritePrompt($"{i + 1}. {ClientMemory.Games[i].Name} ({ClientMemory.Games[i].Players.Count}/{ClientMemory.Games[i].MinimumPlayers})");
        
        // TODO: Allows to the player to return to the main menu
        ConsoleUI.WritePrompt($"{ClientMemory.Games.Count + 1}. Return to the main menu.\n");
        
        ConsoleUI.WritePrompt("Enter the number of the game you want to join: ");
        
        if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= ClientMemory.Games.Count)
        {
            var selectedGame = ClientMemory.Games[choice - 1];
            var command = new JoinGameCommand { PlayerId = ClientIdentity.Id, GameId = selectedGame.Id };
            ClientMemory.CurrentGame = await _lobbyConnection.InvokeAsync<GameDto?>(nameof(ILobbyHubServer.JoinGame), command);

            if (ClientMemory.CurrentGame is null)
            {
                ConsoleUI.WriteError($"Cannot join game {selectedGame.Name}. Shoot again!");
                await DisplayAndJoinGameAsync();
            }
            
            ConsoleUI.WriteInfo($"🔗 You have joined the game {selectedGame.Name}");

            await StartGameAsync();
        }
        else
        {
            ConsoleUI.WriteError("Choice is invalid.");
            await DisplayAndJoinGameAsync();
        }
    }

    private async Task CreatePlayerCompanyAsync()
    {
        ConsoleUI.WriteHeader("Create your company");
        ConsoleUI.WritePrompt("Enter the name of the company:");
        var companyName = ConsoleUI.ReadPrompt();
        
    }

    private async Task StartGameAsync()
    {
        // TODO: Improve this null check
        if (ClientMemory.CurrentGame is null) return;

        while (ClientMemory.CurrentGame.Players.Count < ClientMemory.CurrentGame.MinimumPlayers)
        {

        }

        // Round-play isn't implemented yet - GameDto intentionally omits Rounds (see design.md).

        await Task.CompletedTask;
    }

    public async Task SendAsync(string methodName, params object[] args)
    {
        if (_lobbyConnection.State != HubConnectionState.Connected)
        {
            ConsoleUI.WriteError($"Cannot invoke '{methodName}': connection state = {_lobbyConnection.State}");
            return;
        }

        try
        {
            await _lobbyConnection.InvokeCoreAsync(methodName, args);
        }
        catch (HubException hex)
        {
            ConsoleUI.WriteError($"HubException while invoking '{methodName}': {hex.Message}");
        }
        catch (InvalidOperationException iox)
        {
            ConsoleUI.WriteError($"Invalid operation during '{methodName}': {iox.Message}");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Unexpected error while sending '{methodName}': {ex.Message}");
        }
    }

    public async Task<T?> SendAsync<T>(string methodName, params object[] args)
    {
        if (_lobbyConnection.State != HubConnectionState.Connected)
        {
            ConsoleUI.WriteError($"Cannot invoke '{methodName}': connection state = {_lobbyConnection.State}");
            return default;
        }

        try
        {
            return await _lobbyConnection.InvokeCoreAsync<T>(methodName, args);
        }
        catch (HubException hex)
        {
            ConsoleUI.WriteError($"HubException while invoking '{methodName}': {hex.Message}");
        }
        catch (InvalidOperationException iox)
        {
            ConsoleUI.WriteError($"Invalid operation during '{methodName}': {iox.Message}");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Unexpected error while sending '{methodName}': {ex.Message}");
        }

        return default;
    }
    public async Task DisconnectAsync()
    {
        await _lobbyConnection.StopAsync();
        ConsoleUI.WriteInfo("You have been disconnected from the lobby.");
    }
}