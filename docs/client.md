# Structure du Client

[← Retour à l'index](README.md)

`src/SeriousGame.Client` est une application console/TUI qui pilote le flux de lobby décrit dans [architecture.md](architecture.md).

- `App.cs` pilote la navigation : connexion → identification via pseudo → boucle du menu principal. Le menu se répète jusqu'à Quitter — chaque issue d'enrôlement (partie démarrée, échec, aucune partie, retour volontaire) ramène au menu. `App` route sur l'`EnrollmentResult` retourné par `ILobbyServices` (`GameStarting` / `WaitingForPlayers` / `Failed` / `NoGamesAvailable` / `ReturnToMenu`) ; sur `WaitingForPlayers`, il attend le démarrage via `WaitForGameStartAsync` (le point d'entrée de la future boucle de partie — les tours ne sont pas encore implémentés, on revient au menu). Dépend de `ILobbyServices` et `ILogger<App>`, tous deux injectés.
- `State/ClientIdentity.cs` génère/persiste un identifiant joueur (GUID) par exécution dans un fichier local ; `State/ClientMemory.cs` détient l'état mutable côté client (`Games: List<GameDto>`, `CurrentGame: GameDto?`) alimenté par les handlers `ILobbyHubClient` enregistrés dans `LobbyServices`. (Nommé `State/`, pas `Core/`, pour décrire ce qu'il contient vraiment — de l'état/session côté client, pas un scaffolding générique "core".)
- `Services/LobbyServices.cs` implémente `ILobbyServices` ; détient la `HubConnection` vers `/lobby`, enregistre les handlers côté client pour chaque méthode `ILobbyHubClient` (typés sur `GameDto`/`PlayerDto`), et expose les opérations d'enrôlement qui retournent un `EnrollmentResult` — la navigation reste dans `App`. Construit son URL de hub lui-même depuis `IOptions<WebSocketServerOptions>`, et logue (`ILogger<LobbyServices>`) chaque erreur attrapée (échec de connexion, `HubException`, opération invalide) en plus du message affiché à l'utilisateur via `ConsoleUI`. L'attente du démarrage de partie est événementielle : un `TaskCompletionSource` créé avant chaque invoke d'enrôlement (pour ne pas rater un `GameStarting` arrivé tôt sur un thread SignalR) est résolu par le handler `GameStarting`, et `WaitForGameStartAsync` l'attend — remplace l'ancien busy-wait qui gelait l'application.
- `Services/Interfaces/` regroupe les interfaces de service (`ILobbyServices`, `ILobbyService`, `IGameServices`) séparément de leurs implémentations. `ILobbyServices` (le vrai service utilisé) est distinct de `ILobbyService` (interface du prototype `RoomLobbyPrototype.cs`, inutilisé) — noms proches mais sans rapport.
- `UI/ConsoleUI.cs` / `ConsoleAnimator.cs` sont les seuls helpers d'E/S ; toute sortie destinée à l'utilisateur doit passer par eux plutôt que par du `Console.Write` brut.

## Injection de dépendances

`Program.cs` est l'unique composition root : un `ServiceCollection` enregistre `IOptions<WebSocketServerOptions>`, le logging (`AddLogging` avec le provider console), et les services de l'app (`ILobbyServices` → `LobbyServices`, `IGameServices` → `GameServices`, `App`), tous en `Scoped`. Contrairement au Server (où SignalR crée un scope DI par appel de hub, d'où le `Scoped` de `GameService`/`PlayerService`/`LobbyFlowService`), le Client n'a pas de scope fourni par un framework : `Program.Main` en crée un lui-même via `IServiceScopeFactory.CreateScope()` — un seul scope pour toute la durée de l'exécution — et résout `App` depuis ce scope plutôt que depuis le `ServiceProvider` racine.

## Config et textes UI

- **Valeurs de config** (scheme/domaine/port du hub) vivent dans `appsettings.json` sous `WebSocketServer`, injectées via le pattern `IOptions<T>` dans `LobbyServices`, qui construit l'URL du hub à partir de `options.Value` + `HubRoutes.Lobby` — aucune URL codée en dur.
- **Textes UI et ressources** (prompts de menu, en-têtes, littéraux techniques comme le nom de fichier client-id) : la convention resx/Constants (fichiers `.resx`/`Designer.cs`, quels littéraux vont où) est décrite dans `CLAUDE.md`, pour éviter de la dupliquer à deux endroits.
