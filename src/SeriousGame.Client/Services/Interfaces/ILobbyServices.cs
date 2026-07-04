using Client.Services;

namespace Client.Services.Interfaces;

/// <summary>
/// Contrat pour LobbyServices - le flux réel de connexion/création/join de lobby côté client.
/// Les méthodes d'enrôlement retournent un EnrollmentResult : c'est App qui route la navigation.
/// </summary>
public interface ILobbyServices
{
    Task<bool> ConnectAsync();
    Task IdentifyClientAsync(string nickname);
    Task<EnrollmentResult> CreateGameAsync();
    Task<EnrollmentResult> JoinGameAsync();
    Task WaitForGameStartAsync();
    Task SendAsync(string methodName, params object[] args);
    Task<T?> SendAsync<T>(string methodName, params object[] args);
    Task DisconnectAsync();
}
