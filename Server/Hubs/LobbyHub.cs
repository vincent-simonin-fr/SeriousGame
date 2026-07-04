using Microsoft.AspNetCore.SignalR;
using Shared.Abstractions;
using Shared.Models.Dtos;
using Server.Services;
using Shared.Models;

namespace Server.Hubs;

public sealed class LobbyHub : Hub<ILobbyHubClient>, ILobbyHubServer
{
    private readonly GameFlowService _gameFlowService;
    private readonly GameService _gameService;
    private readonly PlayerService _playerService;

    public LobbyHub(GameFlowService gameFlowService, GameService gameService, PlayerService playerService)
    {
        _gameService = gameService;
        _playerService = playerService;
        _gameFlowService = gameFlowService;
    }

    public override async Task OnConnectedAsync()
    {
        await _gameFlowService.NotifyLobbyOnConnection(Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public async Task IdentifyNewPlayer(CreatePlayerCommand command)
    {
        var player = _playerService.CreatePlayer(command.PlayerId, command.Nickname, Context.ConnectionId);
        await Clients.Caller.Notify($"Hi {command.Nickname}, welcome to the game!");
    }
    
    public async Task UpdatePlayerConnectionId(string clientId)
    {
        var hasPlayer = _playerService.HasPlayer(clientId);
        if (hasPlayer)
        {
            _playerService.UpdateConnectionId(clientId, Context.ConnectionId);
        }
        await Task.CompletedTask;
    }

    public async Task SendMessageToAllPlayers(string message)
    {
        await Clients.Others.Notify($"{message}");
    }

    public async Task<Game> CreateGame(CreateGameCommand command)
    {
        var player = _playerService.GetPlayerById(command.PlayerId);
        var game = _gameService.CreateGame(command.GameName, player);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
        // notify all clients that the game list has changed
        await Clients.Others.GamesUpdated(_gameService.GetGamesNotStarted());
        
        return game;
    }
    
    public async Task<Game?> JoinGame(JoinGameCommand command)
    {
        var player = _playerService.GetPlayerById(command.PlayerId);
        var joined = _gameService.JoinGame(command.GameId, player);
        if (!joined) return null;

        await Groups.AddToGroupAsync(Context.ConnectionId, command.GameId);
        
        var game = _gameService.GetGame(command.GameId);
        
        // When the number of players is equal to MinimumPlayer,
        // then the game starts
        if (game.Players.Count < game.MinimumPlayers)
        {
            await Clients.Group(command.GameId).WaitingForPlayers(game,  game.Players.Select(p => p.Nickname).ToList());
            return game;
        }
        
        game.IsInProgress = true;
        await Clients.All.GamesUpdated(_gameService.GetGamesNotStarted());
        await Clients.Group(command.GameId).GameStarting(game);
        return game;
        
        // Notifier les membres du groupe que le joueur a rejoint
        // await Clients.Group(gameId).Notify("PlayerJoined", player.Nickname);

        // Mettre aussi à jour le lobby (liste des game) pour tous
        // await Clients.All.SendAsync("ReceiveGames", _gameService.GetAllGames());
        // return true;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        
        var game = _playerService.RemovePlayerFromGame(Context.ConnectionId);

        if (game is not null)
        {
            await Clients.Others.GamesUpdated(_gameService.GetGamesNotStarted());
            _playerService.RemovePlayerByConnectionId(Context.ConnectionId);
            
            if (game.IsInProgress)
            {
                await Clients.Group(game.Id).UpdateGameInProgressWhenPlayerQuits(game);
            }
            
            if(game is { IsInProgress: false, Players.Count: > 0 })
            {
                await Clients.Group(game.Id).WaitingForPlayers(game,  game.Players.Select(p => p.Nickname).ToList());
            }
        }
        
        _playerService.RemovePlayerByConnectionId(Context.ConnectionId);
        
        await Clients.Caller.Notify($"Oh sorry, you have been disconnected from the game ! We hope to see you again soon!");
    }
}