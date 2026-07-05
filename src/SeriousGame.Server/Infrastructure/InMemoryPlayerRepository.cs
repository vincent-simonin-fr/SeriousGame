using Server.Application.Abstractions;
using Server.Domain;

namespace Server.Infrastructure;

public class InMemoryPlayerRepository : IPlayerRepository
{
    private readonly AppMemory _appMemory;

    public InMemoryPlayerRepository(AppMemory appMemory)
    {
        _appMemory = appMemory;
    }

    public IEnumerable<Player> GetAll() => _appMemory.Players;
    public void Add(Player player) => _appMemory.Players.Add(player);
    public void Remove(Player player) => _appMemory.Players.Remove(player);
}
