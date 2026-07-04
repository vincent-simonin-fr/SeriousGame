using System.Collections.Concurrent;
using System.Collections.Immutable;
using Client.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Client.Services;

// Unused prototype: a self-contained, in-memory "room" lobby (Guid-keyed, no link to the real
// Game/Player models or the actual SignalR hub). Superseded by LobbyServices, which is what
// GameClientApp actually uses. Kept for reference, not wired into the app - see CLAUDE.md.

/// <summary>
/// Lightweight DTO exposed to callers (immutable view).
/// </summary>
public sealed record GameRoomDto(Guid Id, string Name, string OwnerClientId, ImmutableList<string> Players, int MaxPlayers);

/// <summary>
/// Event args published when lobby rooms change. Contains a snapshot to avoid callers mutating internal state.
/// </summary>
public sealed class RoomsChangedEventArgs : EventArgs
{
    public IReadOnlyCollection<GameRoomDto> RoomsSnapshot { get; }

    public RoomsChangedEventArgs(IEnumerable<GameRoomDto> rooms)
    {
        RoomsSnapshot = rooms.ToList().AsReadOnly();
    }
}

/// <summary>
/// Robust, concurrent lobby implementation:
/// - Uses ConcurrentDictionary for room lookup.
/// - Uses per-room SemaphoreSlim to avoid global locking.
/// - Publishes RoomsChanged events with snapshots.
/// - Performs validation and logs important transitions.
/// </summary>
public sealed class LobbyService : ILobbyService, IDisposable
{
    readonly ILogger<LobbyService> _logger;
    readonly ConcurrentDictionary<Guid, LobbyRoom> _rooms = new();
    readonly ConcurrentDictionary<Guid, SemaphoreSlim> _roomLocks = new();
    bool _disposed;

    public event EventHandler<RoomsChangedEventArgs>? RoomsChanged;

    public LobbyService(ILogger<LobbyService> logger)
    {
        _logger = logger;
    }

    public IEnumerable<GameRoomDto> GetAllRooms()
        => _rooms.Values.Select(ToDto).ToList();

    public bool TryGetRoom(Guid roomId, out GameRoomDto? room)
    {
        if (_rooms.TryGetValue(roomId, out var internalRoom))
        {
            room = ToDto(internalRoom);
            return true;
        }

        room = null;
        return false;
    }

    public async Task<GameRoomDto> CreateRoomAsync(string ownerClientId, string name, int maxPlayers = 4, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ownerClientId)) throw new ArgumentException("ownerClientId required", nameof(ownerClientId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name required", nameof(name));
        if (maxPlayers < 1) throw new ArgumentOutOfRangeException(nameof(maxPlayers));

        var id = Guid.NewGuid();
        var room = new LobbyRoom(id, name.Trim(), ownerClientId, maxPlayers);

        if (!_rooms.TryAdd(id, room))
            throw new InvalidOperationException("Failed to create room due to concurrent modification.");

        // create semaphore for the room
        _roomLocks.TryAdd(id, new SemaphoreSlim(1, 1));

        _logger.LogInformation("Room created {RoomId} ({Name}) by {Owner}", id, name, ownerClientId);
        PublishRoomsChangedSnapshot();

        // return an immutable snapshot
        return ToDto(room);
    }

    public async Task<bool> TryJoinRoomAsync(Guid roomId, string clientId, CancellationToken cancellationToken = default)
    {
        if (clientId is null) throw new ArgumentNullException(nameof(clientId));
        if (!_rooms.TryGetValue(roomId, out var room)) return false;

        var sem = _roomLocks.GetOrAdd(roomId, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (room.Players.Contains(clientId)) return true; // idempotent
            if (room.Players.Count >= room.MaxPlayers) return false;

            room.Players.Add(clientId);
            _logger.LogInformation("Client {ClientId} joined room {RoomId}", clientId, roomId);
        }
        finally
        {
            sem.Release();
        }

        PublishRoomsChangedSnapshot();
        return true;
    }

    public async Task LeaveRoomAsync(Guid roomId, string clientId, CancellationToken cancellationToken = default)
    {
        if (clientId is null) throw new ArgumentNullException(nameof(clientId));
        if (!_rooms.TryGetValue(roomId, out var room)) return;

        var sem = _roomLocks.GetOrAdd(roomId, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!room.Players.Remove(clientId)) return;

            // if owner left and there are players remaining, promote first player to owner
            if (room.OwnerClientId == clientId)
            {
                room.OwnerClientId = room.Players.FirstOrDefault() ?? string.Empty;
            }

            _logger.LogInformation("Client {ClientId} left room {RoomId}", clientId, roomId);
        }
        finally
        {
            sem.Release();
        }

        // remove empty rooms to keep model tidy
        if (room.Players.Count == 0)
            await RemoveRoomAsync(roomId, cancellationToken).ConfigureAwait(false);
        else
            PublishRoomsChangedSnapshot();
    }

    public async Task<bool> RemoveRoomAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        if (!_rooms.TryRemove(roomId, out var removed)) return false;

        // dispose lock if present
        if (_roomLocks.TryRemove(roomId, out var sem))
        {
            try { sem.Dispose(); } catch { /* ignore */ }
        }

        _logger.LogInformation("Room {RoomId} removed", roomId);
        PublishRoomsChangedSnapshot();
        await Task.CompletedTask;
        return true;
    }

    void PublishRoomsChangedSnapshot()
    {
        try
        {
            var snapshot = _rooms.Values.Select(ToDto).ToList();
            RoomsChanged?.Invoke(this, new RoomsChangedEventArgs(snapshot));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish RoomsChanged event");
        }
    }

    static GameRoomDto ToDto(LobbyRoom r)
        => new(r.Id, r.Name, r.OwnerClientId ?? string.Empty, r.Players.ToImmutableList(), r.MaxPlayers);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var sem in _roomLocks.Values)
        {
            try { sem.Dispose(); } catch { /* ignore */ }
        }

        _roomLocks.Clear();
        _rooms.Clear();
    }

    // internal mutable model protected by per-room semaphore
    sealed class LobbyRoom
    {
        public Guid Id { get; }
        public string Name { get; }
        public string OwnerClientId { get; set; }
        public HashSet<string> Players { get; }
        public int MaxPlayers { get; }

        public LobbyRoom(Guid id, string name, string ownerClientId, int maxPlayers)
        {
            Id = id;
            Name = name;
            OwnerClientId = ownerClientId;
            MaxPlayers = maxPlayers;
            Players = new HashSet<string>(StringComparer.Ordinal);
            if (!string.IsNullOrEmpty(ownerClientId))
                Players.Add(ownerClientId);
        }
    }
}