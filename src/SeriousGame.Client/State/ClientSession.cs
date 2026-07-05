using Shared.Models.Dtos;

namespace Client.State;

/// <summary>
/// État de session du client : identité du joueur (persistée dans un fichier client_id_*.txt)
/// et état du lobby. Injectée en Scoped - une instance pour toute l'exécution.
/// </summary>
public class ClientSession
{
    public string PlayerId { get; }
    public string? Nickname { get; set; }
    public List<GameDto> Games { get; set; } = [];
    public GameDto? CurrentGame { get; set; }

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
}
