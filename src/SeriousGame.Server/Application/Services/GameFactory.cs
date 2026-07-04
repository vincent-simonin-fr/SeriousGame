using Shared.Models;

namespace Server.Application.Services;

/// <summary>
/// Encapsule les valeurs par défaut de création d'un Game, auparavant codées en dur dans GameService.CreateGame.
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
