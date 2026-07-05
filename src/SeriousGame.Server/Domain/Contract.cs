using Server.Domain.Base;
using Server.Domain.Enums;

namespace Server.Domain;

/// <summary>
/// Appel d'offres remporté et en cours d'exécution par une entreprise. S'étale sur plusieurs tours
/// (RemainingRounds décrémenté à chaque calcul de tour) et immobilise les consultants affectés
/// jusqu'à la fin, où le budget du tender est versé à la trésorerie.
/// </summary>
public class Contract : BaseModel
{
    public required Company Company { get; init; }
    public required Tender Tender { get; init; }
    public ICollection<Consultant> AssignedConsultants { get; init; } = [];
    public int RemainingRounds { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Active;
}
