using Shared.Models;

namespace Server.Application.Abstractions;

public interface IGameRepository
{
    IEnumerable<Game> GetAll();
    void Add(Game game);
    void Remove(Game game);
}
