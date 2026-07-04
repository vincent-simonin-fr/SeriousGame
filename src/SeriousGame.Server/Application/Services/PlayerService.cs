using Server.Application.Abstractions;
using Shared.Models;

namespace Server.Application.Services;

public class PlayerService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IGameRepository _gameRepository;

    public PlayerService(IPlayerRepository playerRepository, IGameRepository gameRepository)
    {
        _playerRepository = playerRepository;
        _gameRepository = gameRepository;
    }

    public Player GetPlayerByConnectionId(string connectionId)
    {
        var player = _playerRepository.GetAll().FirstOrDefault(p => p.ConnectionId == connectionId);

        if (player is null)
        {
            throw new KeyNotFoundException($"Player with id {connectionId} not found");
        }

        return player;
    }

    public Game? RemovePlayerFromGame(string connectionId)
    {
        var player = _playerRepository.GetAll().First(p => p.ConnectionId == connectionId);
        var game = _gameRepository.GetAll().FirstOrDefault(g => g.Players.Select(p => p.Id).Contains(player.Id));

        if (game?.Players.Count == 1)
        {
            _gameRepository.Remove(game);
        }

        game?.Players.Remove(player);

        return game;
    }

    public bool RemovePlayerByConnectionId(string connectionId)
    {
        try
        {
            var player = _playerRepository.GetAll().First(p => p.Id == connectionId);
            _playerRepository.Remove(player);
            return true;
        }
        catch (Exception)
        {
            // TODO : Log
            return false;
        }
    }

    public Player GetPlayerById(string playerId)
    {
        var player = _playerRepository.GetAll().FirstOrDefault(p => p.Id == playerId);

        if (player is null)
        {
            throw new KeyNotFoundException($"Player with id {playerId} not found");
        }

        return player;
    }

    public bool HasPlayer(string playerId)
    {
        return _playerRepository.GetAll().Any(p => p.Id == playerId);
    }

    public void UpdateConnectionId(string playerId, string connectionId)
    {
        _playerRepository.GetAll().First(p => p.Id == playerId).ConnectionId = connectionId;
    }

    public Player CreatePlayer(string playerId, string playerName, string connectionId)
    {
        var player = new Player
        {
            Id = playerId,
            Nickname = playerName,
            ConnectionId = connectionId
        };

        _playerRepository.Add(player);

        return player;
    }
}
