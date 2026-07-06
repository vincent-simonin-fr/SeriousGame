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

    public void SetSalaryRequirement(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "La prétention salariale ne peut pas être négative.");
        SalaryRequirement = amount;
    }
}