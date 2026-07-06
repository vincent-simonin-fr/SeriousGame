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

    public IEnumerable<Player> GetAll() => _appMemory.Players.Values;
    public void Add(Player player) => _appMemory.Players[player.Id] = player;
    public void Remove(Player player) => _appMemory.Players.TryRemove(player.Id, out _);
}
