# SeriousGame

Petit jeu de lobby multijoueur construit comme exercice pédagogique de Clean Architecture, SOLID, et quelques design patterns : un serveur ASP.NET Core SignalR plus un client console/TUI, partageant DTOs/contrats via un projet commun.

## Démarrage rapide

Nécessite le [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`global.json` fixe la version `10.0.101`).

```bash
dotnet build                                                        # build de toute la solution
dotnet test                                                         # lance tous les projets de tests
dotnet run --project src/SeriousGame.Server --launch-profile https  # lance le serveur, https://localhost:7289
dotnet run --project src/SeriousGame.Client                         # lance le client CLI/TUI
```

## Documentation

Voir **[docs/README.md](docs/README.md)** pour l'architecture, la structure des projets, et les limitations connues — organisée en une page Markdown liée par sujet.
