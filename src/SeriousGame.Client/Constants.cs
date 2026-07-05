namespace Client;

/// <summary>
/// Valeurs littérales hors UI qui ne sont pas de la config (voir appsettings.json / WebSocketServerOptions pour ça).
/// </summary>
public static class Constants
{
    public const string ClientIdFilePattern = "client_id_{0}.txt";
    public const string ClientIdFileSearchPattern = "client_id_*.txt";
}
