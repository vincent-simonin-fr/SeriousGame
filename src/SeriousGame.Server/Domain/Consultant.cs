using Server.Domain.Base;

namespace Server.Domain;

public class Consultant : BaseModel
{
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public int SalaryRequirement { get; private set; }
    public required Company Company { get; set; }
    public ICollection<Skill> Skills { get; } = [];

    public string FullName => $"{Firstname} {Lastname}";
}