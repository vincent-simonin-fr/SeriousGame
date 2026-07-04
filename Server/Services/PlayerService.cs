using Shared.Models;

namespace Server.Services;

public class PlayerService
{
    private readonly AppMemory _appMemory;

    public PlayerService(AppMemory appMemory)
    {
        _appMemory = appMemory;
    }
    
    public Player GetPlayerByConnectionId(string connectionId)
    {
        var player = _appMemory.Players.FirstOrDefault(p => p.ConnectionId == connectionId);

        if (player is null)
        {
            throw new KeyNotFoundException($"Player with id {connectionId} not found");
        }
        
        return player;
    }
    
    public Game? RemovePlayerFromGame(string connectionId)
    {
        var player = _appMemory.Players.First(p => p.ConnectionId == connectionId);
        var game = _appMemory.Games.FirstOrDefault(g => g.Players.Select(p => p.Id).Contains(player.Id));

        if (game?.Players.Count == 1)
        {
            _appMemory.Games.Remove(game);
        }

        game?.Players.Remove(player);

        return game;
    }

    public bool RemovePlayerByConnectionId(string connectionId)
    {
        try
        {
            var player = _appMemory.Players.First(p => p.Id == connectionId);
            _appMemory.Players.Remove(player);
            return true;
        }
        catch (KeyNotFoundException kex)
        {
            // TODO : Log
            return false;
        }
        catch (Exception ex)
        {
            // TODO : Log
            return false;
        }
    }
    
    public Player GetPlayerById(string playerId)
    {
        var player = _appMemory.Players.FirstOrDefault(p => p.Id == playerId);

        if (player is null)
        {
            throw new KeyNotFoundException($"Player with id {playerId} not found");
        }
        
        return player;
    }

    public bool HasPlayer(string playerId)
    {
        return _appMemory.Players.Any(p => p.Id == playerId);
    }

    public void UpdateConnectionId(string playerId, string connectionId)
    {
        _appMemory.Players.First(p => p.Id == playerId).ConnectionId = connectionId;
    }

    public Player CreatePlayer(string playerId, string playerName, string connectionId)
    {
        var player = new Player
        {
            Id = playerId, 
            Nickname = playerName,
            ConnectionId = connectionId
        };
        
        _appMemory.Players.Add(player);
        
        return player;
    }
}