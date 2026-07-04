using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using Shared.Abstractions;
using Shared.Models;
using Shared.Models.Dtos;

namespace Server.Services;

public class GameFlowService
{
    private readonly GameService _gameService;
    private readonly PlayerService _playerService;
    private readonly IHubContext<LobbyHub, ILobbyHubClient> _hub;

    public GameFlowService(
        GameService gameService,
        PlayerService playerService,
        IHubContext<LobbyHub, ILobbyHubClient> hubContext)
    {
        _gameService = gameService;
        _playerService = playerService;
        _hub = hubContext;
    }


    // ------------------------------------------------------------------------
    // CREATE GAME
    // ------------------------------------------------------------------------
    public async Task<GameDto> CreateGame(CreateGameCommand cmd, string connectionId)
    {
        var player = _playerService.GetPlayerById(cmd.PlayerId);
        if (player is null)
            throw new InvalidOperationException("Player not found.");

        var game = _gameService.CreateGame(cmd.GameName, player);

        await _hub.Groups.AddToGroupAsync(connectionId, game.Id);
        await NotifyLobbyGamesUpdated();
        
        var gameDto = GameDto.FromEntity(game);
        
        return gameDto;
    }


    // ------------------------------------------------------------------------
    // JOIN GAME
    // ------------------------------------------------------------------------
    public async Task<GameDto?> JoinGame(JoinGameCommand cmd, string connectionId)
    {
        var player = _playerService.GetPlayerById(cmd.PlayerId);
        if (player is null)
            return null;

        var joined = _gameService.JoinGame(cmd.GameId, player);
        if (!joined)
            return null;

        await _hub.Groups.AddToGroupAsync(connectionId, cmd.GameId);

        var game = _gameService.GetGame(cmd.GameId);
        if (game is null)
            return null;

        var gameDto = GameDto.FromEntity(game);
        
        // Pas encore assez de joueurs
        if (game.Players.Count < game.MinimumPlayers)
        {
            await _hub.Clients.Group(game.Id)
                .WaitingForPlayers(gameDto, game.Players.Select(p => p.Nickname).ToList());

            return gameDto;
        }

        // Sinon la partie démarre
        game.IsInProgress = true;

        await NotifyLobbyGamesUpdated();
        await _hub.Clients.Group(game.Id).GameStarting(gameDto);

        return gameDto;
    }


    // PLAYER DISCONNECT
    // ------------------------------------------------------------------------
    public async Task HandlePlayerDisconnect(string connectionId)
    {
        // Retire le joueur de sa partie éventuelle
        var game = _playerService.RemovePlayerFromGame(connectionId);

        if (game != null)
        {
            // Mettre à jour le lobby
            await NotifyLobbyGamesUpdated();

            // Retirer le joueur totalement du système
            _playerService.RemovePlayerByConnectionId(connectionId);
            
            var gameDto = GameDto.FromEntity(game);
            
            // Si la partie était en cours
            if (game.IsInProgress)
            {
                await _hub.Clients.Group(game.Id).UpdateGameInProgressWhenPlayerQuits(gameDto);
            }
            // Sinon, il manque des joueurs
            else if (game.Players.Count > 0)
            {
                await _hub.Clients.Group(game.Id)
                    .WaitingForPlayers(gameDto, game.Players.Select(p => p.Nickname).ToList());
            }
        }

        // Nettoyage final
        _playerService.RemovePlayerByConnectionId(connectionId);

        // Si on peut encore notifier le client (optionnel)
        await SafeNotifyClient(connectionId,
            "Oh sorry, you have been disconnected. We hope to see you soon again!");
    }


    // ------------------------------------------------------------------------
    // NOTIFICATIONS REUTILISABLES
    // ------------------------------------------------------------------------

    public Task NotifyLobbyOnConnection(string connectionId)
    {
        var games = _gameService.GetGamesNotStarted()
            .AsQueryable()
            .Select(GameDto.Projection)
            .ToList();
        return _hub.Clients.Client(connectionId)
            .GamesUpdated(games);
    }
    
    public Task NotifyLobbyGamesUpdated()
    {
        var games = _gameService.GetGamesNotStarted()
            .AsQueryable()
            .Select(GameDto.Projection)
            .ToList();
        return _hub.Clients.All.GamesUpdated(games);
    }

    public async Task SafeNotifyClient(string connectionId, string message)
    {
        try
        {
            await _hub.Clients.Client(connectionId).Notify(message);
        }
        catch
        {
            // Ignorer (le client est peut-être complètement offline)
        }
    }
}