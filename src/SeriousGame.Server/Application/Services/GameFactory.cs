using Server.Domain;
using Server.Options;

namespace Server.Application.Services;

/// <summary>
/// Assemble un Game à partir de son nom, de son propriétaire et des paramètres de partie
/// (pattern Factory). Reste une fonction pure : les options lui sont fournies par l'appelant.
/// </summary>
public static class GameFactory
{
    public static Game Create(string name, Player owner, GameOptions options)
    {
        var game = new Game
        {
            Name = name,
            Owner = owner,
            MinimumPlayers = options.MinimumPlayers,
            MaximumPlayers = options.MaximumPlayers,
            RoundsNumber = options.RoundsNumber
        };
        game.Players.Add(owner);
        return game;
    }
}
