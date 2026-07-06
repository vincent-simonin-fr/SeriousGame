using System.Collections.Concurrent;
using Server.Domain;

namespace Server.Infrastructure;

/// <summary>
/// État en mémoire du serveur, partagé entre tous les appels de hub (enregistré en singleton).
/// Les collections sont concurrentes : plusieurs connexions SignalR peuvent muter Players/Games
/// en parallèle. Clé = Id de l'entité, donc réenregistrer une même identité remplace au lieu de dupliquer.
/// </summary>
public class AppMemory
{
    public ConcurrentDictionary<string, Player> Players { get; } = new();
    public ConcurrentDictionary<string, Game> Games { get; } = new();

    // Référentiel de compétences (donnée de seed, en lecture seule).
    public IReadOnlyList<Skill> Skills { get; } =
    [
        new Skill { Id = 1, Name = "HTML" },
        new Skill { Id = 2, Name = "CSS" },
        new Skill { Id = 3, Name = "JavaScript" },
        new Skill { Id = 4, Name = "TypeScript" },
        new Skill { Id = 5, Name = "React" },
        new Skill { Id = 6, Name = "Angular" },
        new Skill { Id = 7, Name = "Vue.js" },
        new Skill { Id = 8, Name = "Node.js" },
        new Skill { Id = 9, Name = "Express.js" },
        new Skill { Id = 10, Name = "ASP.NET Core" },
        new Skill { Id = 11, Name = "Ruby on Rails" },
        new Skill { Id = 12, Name = "Django" },
        new Skill { Id = 13, Name = "Flask" },
        new Skill { Id = 14, Name = "PHP" },
        new Skill { Id = 15, Name = "Laravel" },
        new Skill { Id = 16, Name = "Spring Boot" },
        new Skill { Id = 17, Name = "SQL" },
        new Skill { Id = 18, Name = "NoSQL" },
        new Skill { Id = 19, Name = "GraphQL" },
        new Skill { Id = 20, Name = "REST APIs" }
    ];
}
