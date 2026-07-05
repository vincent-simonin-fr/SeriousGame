using Server.Application.Abstractions;
using Server.Domain;

namespace Server.Application.Services;

public class GameService
{
    private readonly IGameRepository _gameRepository;

    public GameService(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public ICollection<Game> GetGamesNotStarted() => _gameRepository.GetAll().Where(g => !g.IsInProgress).ToList();

    public Game CreateGame(string gameName, Player owner)
    {
        var game = GameFactory.Create(gameName, owner);
        _gameRepository.Add(game);
        return game;
    }

    public bool JoinGame(string gameId, Player player)
    {
        var game = _gameRepository.GetAll().FirstOrDefault(r => r.Id == gameId);
        if (game == null) return false;
        game.Players.Add(player);
        return true;
    }

    public bool DisconnectPlayer(string gameId, Player player)
    {
        var game = _gameRepository.GetAll().FirstOrDefault(r => r.Id == gameId);
        if (game == null) return false;
        game.Players.Remove(player);
        _gameRepository.Remove(game);
        return true;
    }

    public bool StartGame(string gameId)
    {
        var game = _gameRepository.GetAll().FirstOrDefault(r => r.Id == gameId);
        if (game == null) return false;
        game.IsInProgress = true;

        return true;
    }

    public Game? GetGame(string gameId) => _gameRepository.GetAll().FirstOrDefault(r => r.Id == gameId);
}
