# Limitations connues

[← Retour à l'index](README.md)

Volontairement hors périmètre, documenté ici plutôt que corrigé en silence :

- **Pas d'authentification/autorisation.** Une revue de sécurité automatisée a trouvé de vrais problèmes ici (`PlayerId`/`clientId` fournis par le client et acceptés sans vérification, pas de `[Authorize]` sur les hubs) — suivi séparément, non corrigé par le travail DTO/layering décrit dans [architecture.md](architecture.md).
- `src/SeriousGame.Client/Services/GameServices.cs` (placeholder pour la future logique client du hub `/game`) est enregistré dans le conteneur DI (`IGameServices`→`GameServices`) pour cohérence avec le reste du câblage (voir [client.md](client.md)), mais reste inutilisé — non injecté dans `App`, qui ne dépend que de `ILobbyServices`.
- Concurrence : les collections d'`AppMemory` (`Players`/`Games`) sont désormais des `ConcurrentDictionary`, mais les **opérations composées** ne sont pas encore synchronisées — `GameService.JoinGame` fait un « trouver-puis-muter » (une partie peut être supprimée entre les deux), et `Game.Players` reste une `List` mutée par des joins/leaves simultanés sur la même partie. À traiter avec les règles de `JoinGame` (verrouillage par agrégat).
- `GameHub` (`/game`) n'a pas été touché — il duplique encore la logique de join avec de simples appels `SendAsync("EventName", ...)` au lieu du pattern `Hub<TClient>` typé utilisé par `LobbyHub`. Voir [server.md](server.md).
