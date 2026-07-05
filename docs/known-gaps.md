# Limitations connues

[← Retour à l'index](README.md)

Volontairement hors périmètre, documenté ici plutôt que corrigé en silence :

- **Pas d'authentification/autorisation.** Une revue de sécurité automatisée a trouvé de vrais problèmes ici (`PlayerId`/`clientId` fournis par le client et acceptés sans vérification, pas de `[Authorize]` sur les hubs) — suivi séparément, non corrigé par le travail DTO/layering décrit dans [architecture.md](architecture.md).
- `src/SeriousGame.Client/Services/GameServices.cs` (placeholder pour la future logique client du hub `/game`) est enregistré dans le conteneur DI (`IGameServices`→`GameServices`) pour cohérence avec le reste du câblage (voir [client.md](client.md)), mais reste inutilisé — non injecté dans `App`, qui ne dépend que de `ILobbyServices`.
- `GameHub` (`/game`) n'a pas été touché — il duplique encore la logique de join avec de simples appels `SendAsync("EventName", ...)` au lieu du pattern `Hub<TClient>` typé utilisé par `LobbyHub`. Voir [server.md](server.md).
