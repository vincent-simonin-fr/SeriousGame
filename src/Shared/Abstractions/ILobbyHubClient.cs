using Shared.Models.Dtos;

namespace Shared.Abstractions;

/// <summary>
/// Represents the actions that the server can trigger
/// and to which the client must subscribe.
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