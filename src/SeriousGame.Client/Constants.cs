namespace Client;

/// <summary>
/// Non-UI literal values that aren't config (see appsettings.json / WebSocketServerOptions for that).
/// </summary>
public static class Constants
{
    public const string ClientIdFilePattern = "client_id_{0}.txt";
    public const string SendMessageToAllPlayersMethod = "SendMessageToAllPlayers";
}
