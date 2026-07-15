# Design patterns utilisés

[← Retour à l'index](README.md)

Recensement des patterns présents dans la solution, à visée pédagogique. Chaque entrée est volontairement concise : intention, incarnation dans le code, et ce qu'elle apporte ici. Les patterns marqués *(idiome .NET)* ne sont pas dans le catalogue GoF mais sont des patterns établis de l'écosystème.

## Patterns GoF

### Observer (comportemental)

**Où** : événements .NET de `LobbyServices` (`WaitingForPlayers`, `GameStarting`, `NotificationReceived`, `ConnectionLost`, `ConnectionRestored`) auxquels `App.SubscribeToLobbyEvents` s'abonne.

**Rôles** : `LobbyServices` est le sujet (il publie quand un broadcast hub arrive), `App` l'observateur (il rafraîchit l'écran). Le sujet ne connaît pas ses observateurs — c'est ce qui permet à `LobbyServices` de rester sans aucune E/S console.

SignalR en est une seconde incarnation, distribuée : le serveur publie (`Clients.Group(...).GameStarting(...)`), les clients abonnés au groupe reçoivent. Même intention (notifier sans connaître les destinataires), à l'échelle du réseau.

### Factory (créationnel)

**Où** : `GameFactory.Create(name, owner, options)` (Server).

**Rôle** : centralise l'assemblage d'un `Game` valide — paramètres de partie issus de la config, propriétaire enrôlé d'office comme premier joueur. Les appelants ne peuvent pas créer une partie « à moitié initialisée ».

### Adapter (structurel)

**Où** : `LobbyHub` (Server).

**Rôle** : adapte l'interface transport SignalR (méthodes de hub, `Context.ConnectionId`) vers l'interface métier `ILobbyFlowService`. Le hub ne contient aucune logique : il parse l'appel, extrait l'identifiant de connexion et délègue. La logique de lobby est ainsi testable et modifiable sans toucher au transport.

### Singleton (créationnel — via DI, pas via instance statique)

**Où** : `AppMemory` (Server), `ClientSession` / `LobbyServices` (Client), enregistrés en `Singleton` dans le conteneur.

**Rôle** : une seule instance partagée pour toute la durée du process (l'état en mémoire du serveur ; l'identité et la connexion hub du client). La variante DI est préférée à la version GoF classique (propriété statique `Instance`) : l'unicité est une décision de composition root, pas une contrainte câblée dans la classe, qui reste donc instanciable en test.

## Patterns d'architecture

### Repository

**Où** : `IGameRepository` / `IPlayerRepository` (Server `Application/Abstractions`) implémentés par `InMemoryGameRepository` / `InMemoryPlayerRepository` (Server `Infrastructure`).

**Rôle** : les services métier manipulent une abstraction de collection sans savoir où elle vit. Le stockage actuel (dictionnaires en mémoire d'`AppMemory`) pourrait être remplacé par une base de données sans toucher à `GameService` / `PlayerService` — c'est la frontière Application ↔ Infrastructure de la Clean Architecture.

### Mapper

**Où** : `Mapper.ToDto(Game)` / `Mapper.ToDto(Player)` (Server `Application/`).

**Rôle** : unique point de traduction Domain → DTO. C'est là que se décide ce qui part sur le réseau — et surtout ce qui n'en part pas (`ConnectionId` est volontairement omis de `PlayerDto`).

### DTO (Data Transfer Object)

**Où** : `GameDto`, `PlayerDto` et les commandes `CreateGameCommand` / `JoinGameCommand` / `CreatePlayerCommand` (`SeriousGame.Shared`).

**Rôle** : le fil n'échange que des types de contrat, jamais les entités de domaine. Le domaine peut évoluer librement côté serveur ; le client ne dépend que de `Shared`.

### Service Layer

**Où** : `LobbyFlowService` orchestrant `GameService` / `PlayerService` (Server) ; `ILobbyServices` → `LobbyServices` (Client).

**Rôle** : chaque cas d'usage (create/join/leave/disconnect) vit en un seul endroit derrière une interface, au lieu d'être éparpillé dans les hubs (côté serveur) ou mêlé à l'E/S console (côté client).

## Idiomes .NET

### Options pattern *(idiome .NET)*

**Où** : `GameOptions` (section `Game` d'appsettings.json, Server) et `WebSocketServerOptions` (section `WebSocketServer`, Client), injectés via `IOptions<T>`.

**Rôle** : la configuration arrive typée et validée à la construction ; aucun service ne lit `IConfiguration` par clés magiques.

### Dependency Injection / composition root

**Où** : `Program.cs` de chaque exécutable — seuls endroits où les implémentations concrètes sont associées aux interfaces.

**Rôle** : toutes les dépendances sont des interfaces reçues par constructeur (inversion de dépendance, le « D » de SOLID). Les durées de vie diffèrent selon l'hôte : Scoped côté serveur (un scope DI par appel de hub SignalR), Singleton côté client (un seul process, un seul flux) — le raisonnement détaillé est dans [client.md](client.md).
