using Server.Domain.Base;

namespace Server.Domain;

public class Game : BaseModel
{
    public required string Name { get; init; }
    public int MinimumPlayers { get; set; } = 3;
    public int MaximumPlayers { get; set; } = 8;
    public int RoundsNumber { get; set; } = 15;
    public bool IsInProgress { get; set; } = false;
    public required Player Owner { get; set; }
    public ICollection<Player> Players { get; set; } = [];
    public ICollection<Company> Companies { get; } = [];

    public ICollection<Round> Rounds { get; } = [];
}