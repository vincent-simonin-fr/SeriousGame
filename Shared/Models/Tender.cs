using Shared.Models.Base;

namespace Shared.Models;

public class Tender : BaseModel
{
    public required string Name { get; init; }
    public ICollection<Skill> Skills { get; } = [];
    public required int Budget {get; init;}
    public required int RoundsNumber {get; init;}
}