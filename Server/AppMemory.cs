using Shared.Models;
using System.Collections.Concurrent;

namespace Server;

public class AppMemory
{
    public ICollection<Player> Players { get; } = [];
    public ICollection<Game> Games { get; } = [];
    // private readonly ConcurrentDictionary<string, Game> _games = new();
    // public ConcurrentBag<Game> game = [];
    public ICollection<Skill> Skills { get; init; } =
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