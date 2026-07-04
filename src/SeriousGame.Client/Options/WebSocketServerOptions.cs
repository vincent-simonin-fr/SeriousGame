namespace Client.Options;

/// <summary>
/// Injecté depuis la section "WebSocketServer" de appsettings.json.
/// </summary>
public class WebSocketServerOptions
{
    public required string Scheme { get; init; }
    public required string Domain { get; init; }
    public required string Port { get; init; }
}
