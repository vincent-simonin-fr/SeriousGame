using Shared.Models.Dtos;

namespace Client.State;

/// <summary>
/// État de session du client : identité du joueur (persistée dans un fichier client_id_*.txt)
/// et état du lobby. Injectée en Singleton - une instance pour toute l'exécution ; les mutations
/// passent par des méthodes nommées plutôt que des setters publics, pour que chaque transition
/// (entrer/quitter une partie...) soit un point d'appel explicite.
/// </summary>
public class ClientSession
{
    public string PlayerId { get; }
    public string? Nickname { get; private set; }
    public List<GameDto> Games { get; private set; } = [];
    public GameDto? CurrentGame { get; private set; }

    public ClientSession()
    {
        // Réutilise l'identité déjà persistée si un fichier existe, sinon en crée une -
        // l'identifiant du joueur reste ainsi stable d'une exécution à l'autre.
        var existingFile = Directory.EnumerateFiles(".", Constants.ClientIdFileSearchPattern).FirstOrDefault();

        if (existingFile is not null)
        {
            PlayerId = File.ReadAllText(existingFile);
        }
        else
        {
            PlayerId = Guid.NewGuid().ToString();
            File.WriteAllText(string.Format(Constants.ClientIdFilePattern, PlayerId), PlayerId);
        }
    }

    public void Identify(string nickname) => Nickname = nickname;

    public void UpdateGames(List<GameDto> games) => Games = games;

    public void EnterGame(GameDto game) => CurrentGame = game;

    public void LeaveCurrentGame() => CurrentGame = null;

    public void MarkPlayerInactiveAfterOtherQuit(GameDto updatedGame)
    {
        if (CurrentGame is null) return;
        CurrentGame.Players.First(p => !updatedGame.Players.Select(player => player.Id).Contains(p.Id)).IsActive = false;
    }
}
