using Shared.Models;
using Shared.Models.Dtos;

namespace SeriousGame.AcceptanceTests;

// Placeholder acceptance coverage: a full Client<->Server SignalR round trip
// is a natural follow-up, not scaffolded here to avoid scope creep.
public class GameDtoMappingTests
{
    [Fact]
    public void MappedGameDto_DoesNotExposeConnectionId()
    {
        var owner = new Player { Id = "p1", Nickname = "Owner", ConnectionId = "secret-connection-id" };
        var game = new Game { Name = "Test Game", Owner = owner };
        game.Players.Add(owner);

        var dto = Mapper.ToDto(game);

        Assert.Equal(owner.Nickname, dto.Owner.Nickname);
        Assert.DoesNotContain("ConnectionId", typeof(PlayerDto).GetProperties().Select(p => p.Name));
    }
}
