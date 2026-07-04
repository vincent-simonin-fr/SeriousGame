using Shared.Models.Dtos;

namespace Server.Application.Abstractions;

/// <summary>
/// Consolidates the lobby create/join/disconnect flow that used to be
/// duplicated between LobbyHub's inline logic and the unused GameFlowService.
/// </summary>
public interface ILobbyFlowService
{
    Task NotifyLobbyOnConnection(string connectionId);
    Task IdentifyNewPlayer(CreatePlayerCommand command, string connectionId);
    Task UpdatePlayerConnectionId(string clientId, string connectionId);
    Task<GameDto> CreateGame(CreateGameCommand command, string connectionId);
    Task<GameDto?> JoinGame(JoinGameCommand command, string connectionId);
    Task HandlePlayerDisconnect(string connectionId);
}
