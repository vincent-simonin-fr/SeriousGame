# Documentation

Ce dossier regroupe la documentation de SeriousGame — un serious game multijoueur où chaque joueur dirige son entreprise dans la tech et doit remporter des appels d'offres pour la développer (celui qui a le plus grand chiffre d'affaires à la fin de la partie gagne), construit comme exercice pédagogique de Clean Architecture / SOLID / design patterns (serveur ASP.NET Core SignalR + client console/TUI).

Une page par sujet, reliée par des liens plutôt que dupliquée :

- **[architecture.md](architecture.md)** — le découpage en 4 projets (`SeriousGame.Server`/`SeriousGame.Client`/`SeriousGame.Shared`), le contrat de communication via DTOs, et les diagrammes de séquence/classes du flux de lobby.
- **[design-patterns.md](design-patterns.md)** — les design patterns utilisés dans la solution, un par un : où ils sont incarnés dans le code et ce qu'ils apportent (Observer, Factory, Adapter, Repository, Mapper, DTO, Service Layer, Options…).
- **[data-model.md](data-model.md)** — le modèle de domaine (`SeriousGame.Shared/Models/`) : entités, relations, cycle de vie d'un tour et décisions de conception.
- **[server.md](server.md)** — structure du projet Server : layering Application/Infrastructure, hubs.
- **[client.md](client.md)** — structure du projet Client : state, services, conventions config/ressources.
- **[known-gaps.md](known-gaps.md)** — bugs connus et failles de sécurité volontairement laissés de côté pour l'instant.

Pour les commandes build/run/test, voir le [README racine](../README.md).
