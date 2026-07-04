namespace Shared;

/// <summary>
/// Chemins de hub partagés entre le Server (MapHub) et le Client (URL de connexion),
/// pour éviter de dupliquer le littéral dans les deux projets.
/// </summary>
public static class HubRoutes
{
    public const string Lobby = "/lobby";
    public const string Game = "/game";
}
