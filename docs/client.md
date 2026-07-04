# Structure du Client

[← Retour à l'index](README.md)

`src/SeriousGame.Client` est une application console/TUI qui pilote le flux de lobby décrit dans [architecture.md](architecture.md).

- `GameClientApp.cs` pilote la boucle console (connexion → identification via pseudo → menu principal → créer/rejoindre une partie).
- `State/ClientIdentity.cs` génère/persiste un identifiant joueur (GUID) par exécution dans un fichier local ; `State/ClientMemory.cs` détient l'état mutable côté client (`Games: List<GameDto>`, `CurrentGame: GameDto?`) alimenté par les handlers `ILobbyHubClient` enregistrés dans `LobbyServices`. (Nommé `State/`, pas `Core/`, pour décrire ce qu'il contient vraiment — de l'état/session côté client, pas un scaffolding générique "core".)
- `Services/LobbyServices.cs` détient la `HubConnection` vers `/lobby`, enregistre les handlers côté client pour chaque méthode `ILobbyHubClient` (typés sur `GameDto`/`PlayerDto`), et expose des méthodes correspondant à `ILobbyHubServer` pour la boucle de l'app. Son `StartGameAsync` attend activement (busy-wait) que `Players.Count < MinimumPlayers` avec une boucle vide et aucun chemin de mise à jour réactive — un bug latent préexistant (rien ne modifie jamais `CurrentGame` une fois posé) ; voir [limitations connues](known-gaps.md).
- `Services/Interfaces/` regroupe les interfaces de service (`ILobbyService`, `IGameServices`) séparément de leurs implémentations.
- `UI/ConsoleUI.cs` / `ConsoleAnimator.cs` sont les seuls helpers d'E/S ; toute sortie destinée à l'utilisateur doit passer par eux plutôt que par du `Console.Write` brut.

## Config et textes UI

- **Valeurs de config** (scheme/domaine/port du hub) vivent dans `appsettings.json` sous `WebSocketServer`, injectées via le pattern `IOptions<T>` : `Options/WebSocketServerOptions.cs` est configuré (`Configure<WebSocketServerOptions>`) depuis `IConfiguration` dans `Program.cs`, résolu via un `ServiceCollection`/`ServiceProvider` minimal (le Client n'a pas d'autre conteneur DI), et injecté dans `GameClientApp` en tant que `IOptions<WebSocketServerOptions>`. L'URL du hub est construite à partir de `options.Value` + `HubRoutes.Lobby` — aucune URL codée en dur.
- **Textes UI et ressources** (prompts de menu, en-têtes, littéraux techniques comme le nom de fichier client-id) : la convention resx/Constants (fichiers `.resx`/`Designer.cs`, quels littéraux vont où) est décrite dans `CLAUDE.md`, pour éviter de la dupliquer à deux endroits.
