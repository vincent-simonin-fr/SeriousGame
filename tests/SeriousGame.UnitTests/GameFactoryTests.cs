using Server.Application.Services;
using Server.Domain;
using Server.Options;

namespace SeriousGame.UnitTests;

public class GameFactoryTests
{
    [Fact]
    public void Create_AddsOwnerAsFirstPlayer()
    {
        var owner = new Player { Id = "p1", Nickname = "Owner", ConnectionId = "c1" };

        var game = GameFactory.Create("Test Game", owner, new GameOptions());

        Assert.Equal("Test Game", game.Name);
        Assert.Single(game.Players);
        Assert.Equal(owner, game.Players.First());
    }

    [Fact]
    public void Create_StampsProvidedGameParameters()
    {
        var owner = new Player { Id = "p1", Nickname = "Owner", ConnectionId = "c1" };
        var options = new GameOptions { MinimumPlayers = 2, MaximumPlayers = 5, RoundsNumber = 10 };

        var game = GameFactory.Create("Test Game", owner, options);

        Assert.Equal(2, game.MinimumPlayers);
        Assert.Equal(5, game.MaximumPlayers);
        Assert.Equal(10, game.RoundsNumber);
    }
}
