using Server.Application.Services;
using Server.Infrastructure;

namespace SeriousGame.IntegrationTests;

public class PlayerServiceTests
{
    private static PlayerService BuildService(out InMemoryPlayerRepository playerRepository)
    {
        var appMemory = new AppMemory();
        playerRepository = new InMemoryPlayerRepository(appMemory);
        var gameRepository = new InMemoryGameRepository(appMemory);
        return new PlayerService(playerRepository, gameRepository);
    }

    [Fact]
    public void RemovePlayerByConnectionId_RemovesPlayerMatchedByConnectionId()
    {
        // Id et ConnectionId sont distincts : la suppression doit matcher le ConnectionId,
        // pas l'Id (le bug historique comparait p.Id, ne trouvait jamais et n'enlevait rien).
        var service = BuildService(out var playerRepository);
        service.CreatePlayer(playerId: "client-1", playerName: "Alice", connectionId: "conn-1");

        var removed = service.RemovePlayerByConnectionId("conn-1");

        Assert.True(removed);
        Assert.Empty(playerRepository.GetAll());
    }

    [Fact]
    public void RemovePlayerByConnectionId_UnknownConnectionId_RemovesNothing()
    {
        var service = BuildService(out var playerRepository);
        service.CreatePlayer(playerId: "client-1", playerName: "Alice", connectionId: "conn-1");

        var removed = service.RemovePlayerByConnectionId("conn-unknown");

        Assert.False(removed);
        Assert.Single(playerRepository.GetAll());
    }
}
