using Shared.Models.Dtos;

namespace Shared.Abstractions;

/// <summary>
/// Represents the actions that the player can trigger
/// </summary>
public interface ILobbyHubServer
{
    Task IdentifyNewPlayer(CreatePlayerCommand command);
    Task UpdatePlayerConnectionId(string clientId);
    Task<GameDto> CreateGame(CreateGameCommand command);
    Task<GameDto?> JoinGame(JoinGameCommand command);
}