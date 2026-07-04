namespace Client.Services;

/// <summary>
/// Issue d'une tentative d'enrôlement (création ou join d'une partie) - App route la navigation dessus.
/// </summary>
public enum EnrollmentResult
{
    /// <summary>La partie démarre immédiatement (quorum atteint).</summary>
    GameStarting,

    /// <summary>Enrôlé, en attente d'autres joueurs - attendre via WaitForGameStartAsync.</summary>
    WaitingForPlayers,

    /// <summary>Le serveur a refusé le join (partie pleine ou déjà démarrée).</summary>
    Failed,

    /// <summary>Aucune partie disponible à rejoindre.</summary>
    NoGamesAvailable,

    /// <summary>Le joueur a choisi de revenir au menu principal.</summary>
    ReturnToMenu
}
