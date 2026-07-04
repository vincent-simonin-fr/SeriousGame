using Server.Application.Services;
using Shared.Models;

namespace SeriousGame.UnitTests;

public class GameFactoryTests
{
    [Fact]
    public void Create_AddsOwnerAsFirstPlayer()
    {
        var owner = new Player { Id = "p1", Nickname = "Owner", ConnectionId = "c1" };

        var game = GameFactory.Create("Test Game", owner);

        Assert.Equal("Test Game", game.Name);
        Assert.Single(game.Players);
        Assert.Equal(owner, game.Players.First());
    }
}
