# Structure du Server

[← Retour à l'index](README.md)

`src/SeriousGame.Server` s'organise en couches Domain / Application / Infrastructure. Le modèle de domaine lui appartient ; seuls les DTOs du contrat sont partagés (voir [architecture.md](architecture.md) et [data-model.md](data-model.md)).

- `Domain/` : le modèle de domaine (`Game`, `Player`, `Company`, `Consultant`, `Skill`, `Tender`, `Training`, `Round`, `Contract`, `TenderApplication`, `TrainingEnrollment`, `Base/BaseModel`, `Enums/*`), namespace `Server.Domain*`. Purement serveur — le Client ne le voit jamais.
- `Application/Abstractions/` : `IGameRepository`, `IPlayerRepository`, `ILobbyFlowService` — les interfaces dont dépend la couche Application.
- `Application/Services/` : `GameService`, `PlayerService` (logique métier, dépendent des interfaces de repository, pas directement de `AppMemory`), `GameFactory` (encapsule les valeurs par défaut de création d'un `Game` — pattern Factory), `LobbyFlowService` (l'**unique** implémentation du flux create/join/disconnect du lobby). `Application/Mapper.cs` centralise la traduction Domain → DTO.
- `Infrastructure/` : `AppMemory` (l'état singleton réellement stocké en mémoire) plus `InMemoryGameRepository`/`InMemoryPlayerRepository` (pattern Repository — de simples enveloppes implémentant les interfaces de la couche Application par-dessus les collections d'`AppMemory`). Pas de persistance réelle (BDD/fichiers) — toujours en mémoire, juste derrière une abstraction.
- `Hubs/` : `LobbyHub` est un **adaptateur fin** — il parse chaque appel SignalR et délègue à `ILobbyFlowService` ; il ne touche jamais `AppMemory`/`GameService`/`PlayerService` directement, et ne contient aucune règle métier. `GameHub` (`/game`) n'a pas été touché, hors périmètre de ce passage de layering — il duplique encore la logique de join avec de simples appels `SendAsync("EventName", ...)` au lieu du pattern `Hub<TClient>` typé utilisé par le lobby.

Config : `appsettings.json` / `appsettings.Development.json`. Les paramètres de partie (`MinimumPlayers`, `MaximumPlayers`, `RoundsNumber`) vivent sous la section `Game`, liés à `Server.Options.GameOptions` via le pattern `IOptions<T>` et posés sur chaque `Game` par `GameFactory` — aucune valeur codée en dur dans le domaine.

Voir [limitations connues](known-gaps.md) pour ce qui est volontairement laissé inachevé.
