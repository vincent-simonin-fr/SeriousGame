# Documentation

Ce dossier regroupe la documentation destinée aux humains pour SeriousGame — un exercice pédagogique de Clean Architecture / SOLID / design patterns, sous forme de petit jeu de lobby multijoueur (serveur ASP.NET Core SignalR + client console/TUI).

Une page par sujet, reliée par des liens plutôt que dupliquée :

- **[architecture.md](architecture.md)** — le découpage en 4 projets (`SeriousGame.Server`/`SeriousGame.Client`/`SeriousGame.Shared`), le contrat de communication via DTOs, les design patterns utilisés (Repository, Factory, Mapper), et les diagrammes de séquence/classes du flux de lobby.
- **[server.md](server.md)** — structure du projet Server : layering Application/Infrastructure, hubs.
- **[client.md](client.md)** — structure du projet Client : state, services, conventions config/ressources.
- **[known-gaps.md](known-gaps.md)** — bugs connus et failles de sécurité volontairement laissés de côté pour l'instant.

Pour les commandes build/run/test, voir le [README racine](../README.md).
