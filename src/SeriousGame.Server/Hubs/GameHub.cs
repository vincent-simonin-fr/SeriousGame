using Microsoft.AspNetCore.SignalR;
using Shared.Models;
using Server.Application.Services;

namespace Server.Hubs;

public class GameHub : Hub
{
    private readonly GameService _gameService;
    private readonly PlayerService _playerService;

    public GameHub(GameService gameService, PlayerService playerService)
    {
        _gameService = gameService;
        _playerService = playerService;
    }

    // Rejoindre une salle ; on ajoute le client au groupe SignalR correspondant
    public async Task<bool> JoinGame(string gameId, Player player)
    {
        var joined = _gameService.JoinGame(gameId, player);
        if (!joined) return false;

        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);

        // Notifier les membres du groupe que le joueur a rejoint
        await Clients.Group(gameId).SendAsync("PlayerJoined", player.Nickname);

        // Mettre aussi à jour le lobby (liste des games) pour tous
        await Clients.All.SendAsync("ReceiveGames", _gameService.GetGamesNotStarted());
        return true;
    }

    // Exemple : envoi de message de chat à la salle
    public async Task SendMessageToPlayerInDaGame(string gameId, string fromNickname, string message)
    {
        await Clients.Group(gameId).SendAsync("ReceiveMessage", fromNickname, message);
    }
}
