using Server.Application.Abstractions;
using Server.Domain;

namespace Server.Infrastructure;

public class InMemoryGameRepository : IGameRepository
{
    private readonly AppMemory _appMemory;

    public InMemoryGameRepository(AppMemory appMemory)
    {
        _appMemory = appMemory;
    }

    public IEnumerable<Game> GetAll() => _appMemory.Games.Values;
    public void Add(Game game) => _appMemory.Games[game.Id] = game;
    public void Remove(Game game) => _appMemory.Games.TryRemove(game.Id, out _);
}
