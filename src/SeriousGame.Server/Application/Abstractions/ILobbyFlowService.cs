using Shared.Models.Dtos;

namespace Server.Application.Abstractions;

/// <summary>
/// Consolide le flux create/join/disconnect du lobby, qui était autrefois
/// dupliqué entre la logique inline de LobbyHub et l'ancien GameFlowService (inutilisé).
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
