namespace Client.Services.Interfaces;

/// <summary>
/// Contrat pour LobbyServices - le flux réel de connexion/création/join de lobby côté client.
/// </summary>
public interface ILobbyServices
{
    Task<bool> ConnectAsync();
    Task IdentifyClientAsync(string nickname);
    Task CreateGameAsync();
    Task DisplayAndJoinGameAsync();
    Task SendAsync(string methodName, params object[] args);
    Task<T?> SendAsync<T>(string methodName, params object[] args);
    Task DisconnectAsync();
}
