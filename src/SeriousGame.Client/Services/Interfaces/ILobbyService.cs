using Client.Services;

namespace Client.Services.Interfaces;

/// <summary>
/// Contrat pour les opérations du domaine lobby. Asynchrone là où les opérations peuvent bloquer ou être attendues par les appelants.
/// Conçu pour être DI-friendly et testable unitairement.
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
