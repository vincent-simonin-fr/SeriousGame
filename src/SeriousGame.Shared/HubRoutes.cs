namespace Shared;

/// <summary>
/// Hub route paths shared between the Server (MapHub) and the Client (connection URL),
/// so the path isn't duplicated as a literal in both projects.
/// </summary>
public static class HubRoutes
{
    public const string Lobby = "/lobby";
    public const string Game = "/game";
}
