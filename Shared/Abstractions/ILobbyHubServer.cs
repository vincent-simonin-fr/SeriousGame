using Shared.Models;
using Shared.Models.Dtos;

namespace Shared.Abstractions;

/// <summary>
/// Represents the actions that the player can trigger
/// </summary>
public interface ILobbyHubServer
{
    Task IdentifyNewPlayer(CreatePlayerCommand command);
    Task UpdatePlayerConnectionId(string clientId);
    Task<Game> CreateGame(CreateGameCommand command);
    Task<Game?> JoinGame(JoinGameCommand command);
}