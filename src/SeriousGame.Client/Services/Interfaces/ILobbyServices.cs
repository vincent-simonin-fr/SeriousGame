using Client.Services;
using Shared.Models.Dtos;

namespace Client.Services.Interfaces;

/// <summary>
/// Contrat pour LobbyServices - transport hub et état de session du lobby, sans aucune E/S console.
/// Les broadcasts serveur sont exposés en événements ; la saisie et l'affichage appartiennent à App.
/// </summary>
public interface ILobbyServices
{
    /// <summary>La partie attend d'autres joueurs (broadcast serveur à chaque arrivée/départ).</summary>
    event Action<GameDto, IReadOnlyList<string>>? WaitingForPlayers;

    /// <summary>Le quorum est atteint, la partie démarre.</summary>
    event Action<GameDto>? GameStarting;

    /// <summary>Message texte poussé par le serveur (Notify).</summary>
    event Action<string>? NotificationReceived;

    /// <summary>Connexion perdue, reconnexion automatique en cours.</summary>
    event Action? ConnectionLost;

    /// <summary>Connexion rétablie et joueur ré-identifié auprès du serveur.</summary>
    event Action? ConnectionRestored;

    /// <summary>URL du hub lobby (pour les messages de connexion affichés par App).</summary>
    string HubUrl { get; }

    Task<bool> ConnectAsync();
    Task IdentifyClientAsync(string nickname);
    Task<EnrollmentResult> CreateGameAsync(string gameName);
    Task<EnrollmentResult> JoinGameAsync(string gameId);

    /// <summary>Attend le démarrage de la partie. Retourne false si le token est annulé avant.</summary>
    Task<bool> WaitForGameStartAsync(CancellationToken cancellationToken);

    /// <summary>Quitte la partie courante côté serveur sans se déconnecter du lobby.</summary>
    Task LeaveGameAsync();

    Task SendAsync(string methodName, params object[] args);
    Task<T?> SendAsync<T>(string methodName, params object[] args);
    Task DisconnectAsync();
}
