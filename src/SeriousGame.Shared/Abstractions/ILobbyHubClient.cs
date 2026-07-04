using Shared.Models.Dtos;

namespace Shared.Abstractions;

/// <summary>
/// Représente les actions que le serveur peut déclencher
/// et auxquelles le client doit s'abonner.
/// </summary>
public interface ILobbyHubClient
{
    Task Notify(string message);
    Task GamesUpdated(ICollection<GameDto> games);
    Task WaitingForPlayers(GameDto game, List<string> playerNames);
    Task UpdateGameInProgressWhenPlayerQuits(GameDto game);
    Task GameStarting(GameDto game);
    Task PublishConnectedPlayersCount(int count);
}