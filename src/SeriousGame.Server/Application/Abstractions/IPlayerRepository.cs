using Server.Domain;

namespace Server.Application.Abstractions;

public interface IPlayerRepository
{
    IEnumerable<Player> GetAll();
    void Add(Player player);
    void Remove(Player player);
}
