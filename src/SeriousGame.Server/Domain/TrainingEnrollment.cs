using Server.Domain.Base;
using Server.Domain.Enums;

namespace Server.Domain;

/// <summary>
/// Consultant mis en formation par son entreprise. S'étale sur plusieurs tours (RemainingRounds
/// décrémenté à chaque calcul de tour) et immobilise le consultant jusqu'à la fin, où la compétence
/// de la formation lui est accordée (certifiée).
/// </summary>
public class TrainingEnrollment : BaseModel
{
    public required Company Company { get; init; }
    public required Consultant Consultant { get; init; }
    public required Training Training { get; init; }
    public int RemainingRounds { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.InProgress;
}
