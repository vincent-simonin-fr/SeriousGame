using Client.Services;

namespace Client.Services.Interfaces;

/// <summary>
/// Contract for lobby domain operations. Async where operations may block or be awaited by callers.
/// Designed to be DI-friendly and unit-testable.
/// </summary>
public interface ILobbyService
{
    event EventHandler<RoomsChangedEventArgs>? RoomsChanged;

    IEnumerable<GameRoomDto> GetAllRooms();
    Task<GameRoomDto> CreateRoomAsync(string ownerClientId, string name, int maxPlayers = 4, CancellationToken cancellationToken = default);
    Task<bool> TryJoinRoomAsync(Guid roomId, string clientId, CancellationToken cancellationToken = default);
    Task LeaveRoomAsync(Guid roomId, string clientId, CancellationToken cancellationToken = default);
    bool TryGetRoom(Guid roomId, out GameRoomDto? room);
    Task<bool> RemoveRoomAsync(Guid roomId, CancellationToken cancellationToken = default);
}
