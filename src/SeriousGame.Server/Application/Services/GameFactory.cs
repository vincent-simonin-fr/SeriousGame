using Shared.Models;

namespace Server.Application.Services;

/// <summary>
/// Encapsulates the defaults a new Game is created with, previously inlined in GameService.CreateGame.
/// </summary>
public static class GameFactory
{
    public static Game Create(string name, Player owner)
    {
        var game = new Game { Name = name, Owner = owner };
        game.Players.Add(owner);
        return game;
    }
}
