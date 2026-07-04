# Architecture

[← Retour à l'index](README.md)

Solution .NET 10 en quatre projets sous `src/` : `SeriousGame.Server` (ASP.NET Core + hubs SignalR, `Sdk="Microsoft.NET.Sdk.Web"`), `SeriousGame.Client` (application console/TUI utilisant `SignalR.Client`), `SeriousGame.Shared` (modèles de domaine/DTOs/abstractions référencés par les deux), plus trois projets xUnit sous `tests/`. Server et Client dépendent uniquement de Shared — jamais l'un de l'autre directement ; les contrats inter-process vivent dans `SeriousGame.Shared/Abstractions` (`ILobbyHubClient`, `ILobbyHubServer`), source de vérité pour les noms/signatures des méthodes de hub.

Les noms de projet/assembly sont `SeriousGame.Client`/`SeriousGame.Server`/`SeriousGame.Shared` (noms de fichiers csproj, dll produites), mais les namespaces C# restent `Client`/`Server`/`Shared` (chaque csproj fixe `<RootNamespace>` explicitement) — le renommage était cosmétique (nom de solution/dll), pas un changement de namespace.

Voir [structure du Client](client.md) et [structure du Server](server.md) pour le détail de chaque projet.

## Contrat de communication : de vrais DTOs, pas les modèles de domaine bruts

`ILobbyHubClient`/`ILobbyHubServer` échangent des `GameDto`/`PlayerDto` (`SeriousGame.Shared/Models/Dtos/`), pas les modèles de domaine `Game`/`Player` directement. `PlayerDto` **omet volontairement `ConnectionId`** (un identifiant de connexion SignalR interne qui fuitait auparavant vers tous les autres clients du lobby). Le mapping est centralisé dans `SeriousGame.Shared/Models/Dtos/Mapper.cs` (`Mapper.ToDto(Game)` / `Mapper.ToDto(Player)`) — c'est le seul endroit qui traduit Domain -> DTO.

`GameDto` n'a volontairement pas de champ `Rounds` (aucune logique de tours de jeu n'existe encore, et `Round` a une référence retour vers `Game` qui nécessiterait de casser le cycle pour sérialiser — à revoir quand le tour de jeu sera implémenté). Les payloads Client -> Server (`CreateGameCommand`, `JoinGameCommand`, `CreatePlayerCommand` dans `SeriousGame.Shared/Models/Requests`) étaient déjà façonnés comme des DTOs et restent inchangés.

## Modèles partagés

`SeriousGame.Shared/Models/*` sont de simples classes mutables (`Game`, `Player`, `Round`, `Skill`, `Company`, `Consultant`, `Tender`, `Training`) — le modèle de domaine, utilisé côté serveur et comme source lue par `Mapper.ToDto`. `SeriousGame.Shared/Models/Dtos/*` (`GameDto`, `PlayerDto`, `Mapper`) sont les contrats réseau envoyés aux clients. `BaseModel` donne aux entités un `Id` de type GUID (string). `SeriousGame.Shared/Models/Requests/*` sont les payloads des commandes de hub (`CreateGameCommand`, `JoinGameCommand`, `CreatePlayerCommand`). `SeriousGame.Shared/HubRoutes.cs` contient les constantes de chemin de hub (`/lobby`, `/game`) pour que le `Program.cs` du Server (`MapHub`) et le constructeur d'URL de hub du Client ne codent pas en dur le même littéral chacun de leur côté.

## Design patterns utilisés (à visée pédagogique)

- **Repository** — `IGameRepository`/`IPlayerRepository` (Server `Application/Abstractions`) au-dessus de `InMemoryGameRepository`/`InMemoryPlayerRepository` (Server `Infrastructure`), de simples enveloppes autour des collections du singleton `AppMemory`.
- **Factory** — `GameFactory.Create` encapsule les valeurs par défaut de création d'un `Game` (le propriétaire comme premier joueur, etc.).
- **Mapper** — `Mapper.ToDto(...)` centralise la traduction Domain → DTO en un seul endroit.

Écart volontaire par rapport à la Clean Architecture "manuel scolaire" : les modèles de domaine (`Game`, `Player`, etc.) vivent dans `Shared`, pas dans un dossier `SeriousGame.Server/Domain`, car ce sont de vrais types partagés sur le réseau entre Client et Server dans cette application multijoueur — les déplacer voudrait dire dupliquer les types ou ajouter une couche de mapping supplémentaire, plus de complexité que ce que ce layering pédagogique nécessite.

## Logique de flux de jeu autrefois dupliquée — maintenant consolidée

La logique inline de `LobbyHub` et l'ancien `GameFlowService` (non enregistré dans le DI, qui réimplémentait indépendamment create/join/disconnect avec `GameDto`) étaient auparavant deux implémentations parallèles, à moitié cassées — c'était la cause d'une vraie erreur de compilation (`LobbyHub` envoyait un `Game` là où `ILobbyHubClient` attendait un `GameDto`). Il n'y a maintenant qu'une seule implémentation : `LobbyFlowService` (adapté de la structure de `GameFlowService`, puisqu'il utilisait déjà `IHubContext`/DTOs), enregistré dans le DI sous `ILobbyFlowService`. Toute modification de la logique de lobby se fait désormais à un seul endroit.

Voir [limitations connues](known-gaps.md) pour ce qui reste hors périmètre.

## Diagramme de séquence : Créer une partie

```
Client
  │
  │   CreateGame(command)
  ▼
LobbyHub
  │     GetPlayerById(command.PlayerId)
  ├──────────────────────────────────────► PlayerService
  │                                       │
  │                                       ▼
  │                         return Player instance
  ◄───────────────────────────────────────┘
  │
  │     CreateGame(command.GameName, player)
  ├──────────────────────────────────────► GameService
  │                                       │
  │                                       ▼
  │                         return new Game
  ◄───────────────────────────────────────┘
  │
  │ Groups.AddToGroupAsync(connectionId, game.Id)
  ├──────────────────────────────────────► SignalR Groups
  │
  │ Clients.Others.GamesUpdated(GetGamesNotStarted())
  ├──────────────────────────────────────► Other Clients
  │
  ▼
Client  ─────────────── receives created Game
```

## Diagramme de séquence : Rejoindre une partie

```
Client
  │
  │   JoinGame(command)
  ▼
LobbyHub
  │     GetPlayerById(command.PlayerId)
  ├──────────────────────────────────────► PlayerService
  │                                       │
  │                                       ▼
  │                        return Player instance
  ◄───────────────────────────────────────┘
  │
  │     JoinGame(command.GameId, player)
  ├──────────────────────────────────────► GameService
  │                                       │
  │                       returns bool (joined or not)
  ◄───────────────────────────────────────┘
  │
  │ if(!joined) return null
  │
  │ Groups.AddToGroupAsync(connectionId, game.Id)
  ├──────────────────────────────────────► SignalR Groups
  │
  │     GetGame(command.GameId)
  ├──────────────────────────────────────► GameService
  │                                       │
  │                                       ▼
  │                              return Game
  ◄───────────────────────────────────────┘
  │
  │  if game.Players < MinimumPlayers :
  │       Clients.Group(game.Id).WaitingForPlayers(game, playersNicknames)
  ├──────────────────────────────────────► Game Group
  │       return game
  │
  │ else :
  │       game.IsInProgress = true
  │       Clients.All.GamesUpdated(...)
  ├──────────────────────────────────────► All Clients
  │       Clients.Group(game.Id).GameStarting(game)
  ├──────────────────────────────────────► Game Group
  │       return game
  ▼
Client  ─────────────── receives the result (Game or null)
```

## Diagramme de séquence : OnDisconnectedAsync

```
SignalR Transport detects disconnect
  │
  ▼
LobbyHub
  │   RemovePlayerFromGame(connectionId)
  ├──────────────────────────────────────► PlayerService
  │                                       │
  │                              returns Game? (null if not in a game)
  ◄───────────────────────────────────────┘
  │
  │ if(game not null) :
  │     Clients.Others.GamesUpdated(...)
  ├──────────────────────────────────────► Other Clients
  │
  │     if(game.IsInProgress)
  │         Clients.Group(game.Id).UpdateGameInProgressWhenPlayerQuits(game)
  ├──────────────────────────────────────► Game Group
  │
  │     if(game.IsInProgress == false AND game.Players.Count > 0)
  │         Clients.Group(game.Id).WaitingForPlayers(game, playersNicknames)
  ├──────────────────────────────────────► Game Group
  │
  │ RemovePlayerByConnectionId(connectionId)
  ├──────────────────────────────────────► PlayerService
  │
  │ Clients.Caller.Notify("Oh sorry, you have been disconnected…")
  ├──────────────────────────────────────► Former Client (if reachable)
  │
  ▼
End
```

## Diagramme de classes : flux de lobby

```
                         ┌───────────────────────────────┐
                         │         LobbyFlowService       │
                         │───────────────────────────────│
                         │ + CreateGame(cmd, connId)     │
                         │ + JoinGame(cmd, connId)       │
                         │ + HandlePlayerDisconnect(id)  │
                         │ - NotifyLobbyGamesUpdated()   │
                         └───────────────▲───────────────┘
                                         │ uses
┌────────────────────────────┐           │             ┌───────────────────────────┐
│        GameService         │◄──────────┘             │       PlayerService       │
│────────────────────────────│                         │───────────────────────────│
│ + CreateGame(name, owner)  │                         │ + CreatePlayer(...)       │
│ + JoinGame(id, player)     │                         │ + GetPlayerById(id)       │
│ + GetGame(id)              │                         │ + RemovePlayerFromGame()  │
└────────────────────────────┘                         └──────────▲────────────────┘
                                                                  │
                                                ┌─────────────────┴──────────────┐
                                                │      Domain Models             │
                                                │────────────────────────────────│
                                                │ Game, Player, Round, etc.       │
                                                └────────────────────────────────┘


            ┌────────────────────────────┐
            │  IHubContext<LobbyHub,...> │
            │────────────────────────────│
            │ + Clients.All              │
            │ + Clients.Group(groupName) │
            │ + Clients.Client(connId)   │
            └───────────────▲────────────┘
                            │ injected into
              ┌─────────────┴───────────────┐
              │        LobbyFlowService     │
              └─────────────────────────────┘
```
