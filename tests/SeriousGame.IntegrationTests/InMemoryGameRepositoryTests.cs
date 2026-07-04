using Server.Infrastructure;
using Shared.Models;

namespace SeriousGame.IntegrationTests;

public class InMemoryGameRepositoryTests
{
    [Fact]
    public void AddedGame_IsReturnedByGetAll()
    {
        var appMemory = new AppMemory();
        var repository = new InMemoryGameRepository(appMemory);
        var owner = new Player { Id = "p1", Nickname = "Owner", ConnectionId = "c1" };
        var game = new Game { Name = "Test Game", Owner = owner };

        repository.Add(game);

        Assert.Contains(game, repository.GetAll());
    }
}
