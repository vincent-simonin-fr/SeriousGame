namespace Server.Options;

/// <summary>
/// Paramètres de partie, injectés depuis la section "Game" de appsettings.json.
/// Les valeurs par défaut servent de repli si la section est absente.
/// </summary>
public class GameOptions
{
    public int MinimumPlayers { get; init; } = 3;
    public int MaximumPlayers { get; init; } = 8;
    public int RoundsNumber { get; init; } = 15;
}
