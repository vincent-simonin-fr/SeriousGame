using Shared.Models.Base;

namespace Shared.Models;

public class Game : BaseModel
{
    public required string Name { get; init; }
    public int MinimumPlayers { get; set; } = 3;
    public int MaximumPlayers { get; set; } = 8;
    public int RoundsNumber { get; set; } = 15;
    public bool IsInProgress { get; set; } = false;
    public required Player Owner { get; set; }
    public ICollection<Player> Players { get; set; } = [];

    public ICollection<Round> Rounds { get; } = [];
}