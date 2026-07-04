using Shared.Models;
using Shared.Models.Dtos;

namespace SeriousGame.AcceptanceTests;

// Couverture d'acceptation placeholder : un aller-retour SignalR complet Client<->Server
// est un suivi naturel, non scaffoldé ici pour éviter le scope creep.
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
