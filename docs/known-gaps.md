# Limitations connues

[← Retour à l'index](README.md)

Volontairement hors périmètre, documenté ici plutôt que corrigé en silence :

- **Pas d'authentification/autorisation.** Une revue de sécurité automatisée a trouvé de vrais problèmes ici (`PlayerId`/`clientId` fournis par le client et acceptés sans vérification, pas de `[Authorize]` sur les hubs, contournement du certificat TLS en dev dans `src/SeriousGame.Client/Services/HubConnectionManager.cs`) — suivi séparément, non corrigé par le travail DTO/layering décrit dans [architecture.md](architecture.md).
- `src/SeriousGame.Client/Services/HubConnectionManager.cs` (gestionnaire de connexion multi-hub générique — référence un hub `/chat` qui n'existe même pas côté serveur) et `src/SeriousGame.Client/Services/GameServices.cs` (placeholder pour la future logique client du hub `/game`) restent inutilisés — `GameClientApp`/`LobbyServices` instancient toujours `LobbyServices` directement. `src/SeriousGame.Client/Services/RoomLobbyPrototype.cs` et son `ILobbyService`/`LobbyService` sans rapport (concept de "room" indexé par `Guid`) sont eux aussi inutilisés, un prototype abandonné déconnecté des vrais modèles `Game`/`Player` — à ne pas confondre avec le vrai flux de lobby.
- `PlayerService.RemovePlayerByConnectionId` compare encore `p.Id == connectionId` (devrait être `p.ConnectionId`) — un bug logique préexistant connu, volontairement non corrigé ici (lié au problème de confiance sur l'identité de connexion ci-dessus).
- `LobbyServices.StartGameAsync` (Client) attend activement (busy-wait) que `Players.Count < MinimumPlayers` avec une boucle vide et aucun chemin de mise à jour réactive — peut geler l'application console après avoir créé/rejoint une partie avec trop peu de joueurs. Voir [client.md](client.md).
- `GameHub` (`/game`) n'a pas été touché — il duplique encore la logique de join avec de simples appels `SendAsync("EventName", ...)` au lieu du pattern `Hub<TClient>` typé utilisé par `LobbyHub`. Voir [server.md](server.md).
