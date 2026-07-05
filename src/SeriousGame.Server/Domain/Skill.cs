using Server.Domain.Enums;

namespace Server.Domain;

public class Skill
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public Level Level { get; private set; } = Level.Zero;
}