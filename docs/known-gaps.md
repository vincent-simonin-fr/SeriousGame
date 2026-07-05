# Limitations connues

[← Retour à l'index](README.md)

Volontairement hors périmètre, documenté ici plutôt que corrigé en silence :

- **Pas d'authentification/autorisation.** Une revue de sécurité automatisée a trouvé de vrais problèmes ici (`PlayerId`/`clientId` fournis par le client et acceptés sans vérification, pas de `[Authorize]` sur les hubs) — suivi séparément, non corrigé par le travail DTO/layering décrit dans [architecture.md](architecture.md).
- `src/SeriousGame.Client/Services/GameServices.cs` (placeholder pour la future logique client du hub `/game`) est enregistré dans le conteneur DI (`IGameServices`→`GameServices`) pour cohérence avec le reste du câblage (voir [client.md](client.md)), mais reste inutilisé — non injecté dans `App`, qui ne dépend que de `ILobbyServices`.
- `PlayerService.RemovePlayerByConnectionId` compare encore `p.Id == connectionId` (devrait être `p.ConnectionId`) — un bug logique préexistant connu, volontairement non corrigé ici (lié au problème de confiance sur l'identité de connexion ci-dessus). Conséquence visible depuis que l'identité client est réellement persistée : les `Player` s'accumulent côté serveur avec le même `Id`, et `GetPlayerById` retournant le premier trouvé, le **pseudo de la toute première identification gagne** pour les sessions suivantes du même client tant que le serveur n'a pas redémarré.
- `GameHub` (`/game`) n'a pas été touché — il duplique encore la logique de join avec de simples appels `SendAsync("EventName", ...)` au lieu du pattern `Hub<TClient>` typé utilisé par `LobbyHub`. Voir [server.md](server.md).
