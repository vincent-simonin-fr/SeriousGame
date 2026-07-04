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

    /// <summary>Attend le démarrage de la partie. Retourne false si le joueur a annulé (Échap).</summary>
    Task<bool> WaitForGameStartAsync();

    /// <summary>Quitte la partie courante côté serveur sans se déconnecter du lobby.</summary>
    Task LeaveGameAsync();
    Task SendAsync(string methodName, params object[] args);
    Task<T?> SendAsync<T>(string methodName, params object[] args);
    Task DisconnectAsync();
}
