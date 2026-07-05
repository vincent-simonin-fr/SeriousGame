using Server.Domain.Base;

namespace Server.Domain;

public class Company : BaseModel
{
    public required string Name { get; set; }
    public required Player PlayerOwner { get; set; }
    public int Treasury { get; private set; } = 1000000;
    public ICollection<Consultant> Staff { get; } = [];

    // État persistant entre tours : appels d'offres en cours d'exécution et consultants en formation.
    // Un consultant est « occupé » s'il figure dans un Contract actif ou un TrainingEnrollment en cours.
    public ICollection<Contract> Contracts { get; } = [];
    public ICollection<TrainingEnrollment> TrainingEnrollments { get; } = [];

    public void Deposit(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Le montant déposé ne peut pas être négatif.");
        Treasury += amount;
    }

    public void Withdraw(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Le montant retiré ne peut pas être négatif.");
        Treasury -= amount;
    }
}