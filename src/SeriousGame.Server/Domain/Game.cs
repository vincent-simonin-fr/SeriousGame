using Server.Domain.Base;

namespace Server.Domain;

public class Game : BaseModel
{
    public required string Name { get; init; }
    public int MinimumPlayers { get; set; }
    public int MaximumPlayers { get; set; }
    public int RoundsNumber { get; set; }
    public bool IsInProgress { get; set; } = false;
    public required Player Owner { get; set; }
    public ICollection<Player> Players { get; set; } = [];
    public ICollection<Company> Companies { get; } = [];

    public ICollection<Round> Rounds { get; } = [];
}