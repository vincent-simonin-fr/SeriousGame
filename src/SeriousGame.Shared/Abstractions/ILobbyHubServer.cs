using Shared.Models.Dtos;

namespace Shared.Abstractions;

/// <summary>
/// Représente les actions que le joueur peut déclencher
/// </summary>
public interface ILobbyHubServer
{
    Task IdentifyNewPlayer(CreatePlayerCommand command);
    Task UpdatePlayerConnectionId(string clientId);
    Task<GameDto> CreateGame(CreateGameCommand command);
    Task<GameDto?> JoinGame(JoinGameCommand command);
}