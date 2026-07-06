namespace Client.Game;

/// <summary>
/// Phases d'un tour côté client, reprises des "Client Actions" du support pédagogique :
/// analyser le marché, simuler, choisir, communiquer - plus une phase de résolution
/// représentant l'attente du résultat du tour calculé côté serveur.
/// </summary>
public enum TurnPhase
{
    MarketAnalysis,
    Simulation,
    Decision,
    Submission,
    Resolution
}
