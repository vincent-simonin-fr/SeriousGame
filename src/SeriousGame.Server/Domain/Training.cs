using Server.Domain.Base;

namespace Server.Domain;

public class Training : BaseModel
{
    public required string Name { get; init; }
    public required Skill Skill { get; init; }
    public int Cost { get; init; }
    public required int RoundsNumber {get; init;}
}