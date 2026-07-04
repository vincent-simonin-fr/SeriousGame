namespace Client.Options;

/// <summary>
/// Bound from the "WebSocketServer" section of appsettings.json.
/// </summary>
public class WebSocketServerOptions
{
    public required string Scheme { get; init; }
    public required string Domain { get; init; }
    public required string Port { get; init; }
}
