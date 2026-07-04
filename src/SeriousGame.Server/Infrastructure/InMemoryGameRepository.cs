using Server.Application.Abstractions;
using Shared.Models;

namespace Server.Infrastructure;

public class InMemoryGameRepository : IGameRepository
{
    private readonly AppMemory _appMemory;

    public InMemoryGameRepository(AppMemory appMemory)
    {
        _appMemory = appMemory;
    }

    public IEnumerable<Game> GetAll() => _appMemory.Games;
    public void Add(Game game) => _appMemory.Games.Add(game);
    public void Remove(Game game) => _appMemory.Games.Remove(game);
}
