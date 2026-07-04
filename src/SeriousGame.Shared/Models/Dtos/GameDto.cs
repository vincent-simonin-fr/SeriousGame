namespace Shared.Models.Dtos;

public class GameDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public int MinimumPlayers { get; init; }
    public int MaximumPlayers { get; init; }
    public int RoundsNumber { get; init; }
    public bool IsInProgress { get; init; }
    public required PlayerDto Owner { get; init; }
    public ICollection<PlayerDto> Players { get; init; } = [];
}
