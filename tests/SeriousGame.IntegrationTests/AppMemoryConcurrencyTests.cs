using Server.Domain;
using Server.Infrastructure;

namespace SeriousGame.IntegrationTests;

public class AppMemoryConcurrencyTests
{
    [Fact]
    public void PlayerRepository_ConcurrentAddsAndReads_StayConsistent()
    {
        var appMemory = new AppMemory();
        var repository = new InMemoryPlayerRepository(appMemory);
        const int count = 2_000;

        // Écritures et lectures concurrentes : avec une List non synchronisée, ceci lèverait
        // (énumération pendant modification) ou perdrait des entrées. Le ConcurrentDictionary tient.
        Parallel.For(0, count, i =>
        {
            repository.Add(new Player { Id = $"p{i}", Nickname = $"P{i}", ConnectionId = $"c{i}" });
            _ = repository.GetAll().Count();
        });

        Assert.Equal(count, repository.GetAll().Count());
    }

    [Fact]
    public void GameRepository_ConcurrentAdds_StoreEveryDistinctGame()
    {
        var appMemory = new AppMemory();
        var repository = new InMemoryGameRepository(appMemory);
        var owner = new Player { Id = "p1", Nickname = "Owner", ConnectionId = "c1" };
        const int count = 2_000;

        Parallel.For(0, count, i => repository.Add(new Game { Name = $"Game {i}", Owner = owner }));

        Assert.Equal(count, repository.GetAll().Count());
    }
}
