using Shared.Models;

namespace Server.Services;

public class GameService
{
    private readonly AppMemory _appMemory;

    public GameService(AppMemory appMemory)
    {
        _appMemory = appMemory;
    }
    
    public ICollection<Game> GetGamesNotStarted() => _appMemory.Games.Where(g => !g.IsInProgress).ToList();

    public Game CreateGame(string gameName, Player owner)
    {
        var game = new Game { Name = gameName,  Owner = owner };
        game.Players.Add(owner);
        _appMemory.Games.Add(game);
        return game;
    }

    public bool JoinGame(string gameId, Player player)
    {
        var game = _appMemory.Games.FirstOrDefault(r => r.Id == gameId);
        if (game == null) return false;
        game.Players.Add(player);
        return true;
    }

    public bool DisconnectPlayer(string gameId, Player player)
    {
        var game = _appMemory.Games.FirstOrDefault(r => r.Id == gameId);
        if (game == null) return false;
        game.Players.Remove(player);
        _appMemory.Games.Remove(game);
        return true;
    }

    public bool StartGame(string gameId)
    {
        var game = _appMemory.Games.FirstOrDefault(r => r.Id == gameId);
        if (game == null) return false;
        game.IsInProgress = true;
        
        return true;
    }
    
    
    
    // TODO Remove
    public Game GetGame(string gameId)
    {
        var game = _appMemory.Games.First(r => r.Id == gameId);
        return game;
    }
}
