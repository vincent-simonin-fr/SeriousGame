# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

Build/run from repo root (`SeriousGame.slnx`) or per-project.

```bash
dotnet build                                          # build whole solution
dotnet test                                           # run all three test projects
dotnet run --project src/Server --launch-profile https   # run WebAPI (SignalR server), https://localhost:7289
dotnet run --project src/Client                           # run CLI/TUI client
```

- Test projects: `tests/SeriousGame.UnitTests`, `tests/SeriousGame.IntegrationTests`, `tests/SeriousGame.AcceptanceTests` (xUnit). Each currently has a small smoke test - deeper test coverage is a natural follow-up, not yet written.
- Server config: `src/Server/appsettings.json` / `appsettings.Development.json`. Client config: `src/Client/appsettings.json` (embedded resource, copied to output).
- Client persists identity to `client_id_<guid>.txt` next to the binary (gitignored via `client_id*.txt`).
- Dockerfiles exist per-project (`src/Client/Dockerfile`, `src/Server/Dockerfile`, build context = repo root) but there is no docker-compose file in the repo despite README references to one — don't assume it exists.
- Solution targets **.NET 10** (`global.json` pins SDK `10.0.101`). Bumped from .NET 9 mid-refactor when `dotnet new xunit` scaffolded the test projects on the newest installed SDK - rather than downgrade the new test projects, `src/Client`/`src/Server`/`src/Shared` and both Dockerfiles were upgraded to match.

## Architecture

Four-project .NET 10 solution under `src/`: `Server` (ASP.NET Core + SignalR hubs, `Sdk="Microsoft.NET.Sdk.Web"`), `Client` (console/TUI app using `SignalR.Client`), `Shared` (domain models/DTOs/abstractions referenced by both), plus three xUnit projects under `tests/`. Server and Client only depend on Shared — never reference each other directly; cross-process contracts live in `Shared/Abstractions` (`ILobbyHubClient`, `ILobbyHubServer`) as the source of truth for hub method names/signatures.

### Wire contract: real DTOs, not raw domain models
`ILobbyHubClient`/`ILobbyHubServer` exchange `GameDto`/`PlayerDto` (`Shared/Models/Dtos/`), not the `Game`/`Player` domain models directly. `PlayerDto` deliberately **omits `ConnectionId`** (an internal SignalR connection id that used to leak to every other client in the lobby). Mapping is centralized in `Shared/Models/Dtos/Mapper.cs` (`Mapper.ToDto(Game)` / `Mapper.ToDto(Player)`) - this is the one place that translates Domain -> DTO. `GameDto` intentionally has no `Rounds` field (no round-play logic exists yet, and `Round` has a back-reference to `Game` that would need cycle-breaking to serialize - revisit when round-play is built). Client -> Server payloads (`CreateGameCommand`, `JoinGameCommand`, `CreatePlayerCommand` in `Shared/Models/Requests`) were already DTO-shaped and are unchanged.

### Server layering: Application / Infrastructure
- `src/Server/Application/Abstractions/`: `IGameRepository`, `IPlayerRepository`, `ILobbyFlowService` - the seams the Application layer depends on.
- `src/Server/Application/Services/`: `GameService`, `PlayerService` (business logic, depend on the repository interfaces, not `AppMemory` directly), `GameFactory` (encapsulates `Game` creation defaults - Factory pattern), `LobbyFlowService` (the **single** implementation of the lobby create/join/disconnect flow - see below).
- `src/Server/Infrastructure/`: `AppMemory` (the actual in-memory singleton state) plus `InMemoryGameRepository`/`InMemoryPlayerRepository` (Repository pattern - thin wrappers implementing the Application-layer interfaces over `AppMemory`'s collections). No real persistence (DB/files) - still in-memory, just behind an abstraction.
- `src/Server/Hubs/`: `LobbyHub` is now a **thin adapter** - it parses each SignalR call and delegates to `ILobbyFlowService`; it does not touch `AppMemory`/`GameService`/`PlayerService` directly, or contain business rules. `GameHub` (`/game`) is untouched, out of scope for this layering pass - it still duplicates join logic with plain `SendAsync("EventName", ...)` calls instead of the strongly-typed `Hub<TClient>` pattern the lobby uses.
- **Deviation from textbook Clean Architecture, done deliberately**: domain models (`Game`, `Player`, etc.) live in `Shared`, not a `Server/Domain` folder, because they're genuinely shared wire types between `Client` and `Server` in this multiplayer app - moving them would mean duplicating types or adding another mapping layer, which is more complexity than this pedagogical layering needs.

### Previously duplicated game-flow logic - now consolidated
`LobbyHub`'s inline logic and the old, DI-unregistered `GameFlowService` (which independently reimplemented create/join/disconnect using `GameDto`) used to be two parallel, half-broken implementations - this was the cause of a real compile error (`LobbyHub` sent `Game` where `ILobbyHubClient` expected `GameDto`). They are now one implementation: `LobbyFlowService` (adapted from `GameFlowService`'s structure, since it already used `IHubContext`/DTOs), registered in DI as `ILobbyFlowService`. If you touch lobby flow logic, it lives in exactly one place now.

### Known gaps, still out of scope
- **No authentication/authorization.** An automated security review found real issues here (client-supplied `PlayerId`/`clientId` trusted without verification, no `[Authorize]` on hubs, dev-only TLS cert bypass in `Client/Services/HubClientManager.cs`) - tracked separately, not fixed by the DTO/layering work above.
- `Client/Services/HubClientManager.cs` (generic multi-hub connection manager) and `Client/Services/GameServices.cs` (empty stub) remain unused - `GameClientApp`/`LobbyServices` still instantiate `LobbyServices` directly. `Client/Services/yo.cs`'s unrelated `ILobbyService`/`LobbyService` (room concept keyed by `Guid`) is also still unused - don't confuse it with the real lobby flow.
- `PlayerService.RemovePlayerByConnectionId` still matches on `p.Id == connectionId` (should be `p.ConnectionId`) - a known pre-existing logic bug, deliberately not fixed here (part of the deferred security/auth follow-up, since it's tangled with connection-identity trust).

### Client structure
- `Client/Core/GameClientApp.cs` drives the console loop (connect → identify nickname → main menu → create/join game).
- `Client/Core/ClientIdentity.cs` generates/persists a per-run GUID-based player id to a local file; `ClientMemory.cs` holds mutable client-side game state (`Games: List<GameDto>`, `CurrentGame: GameDto?`) populated by the `ILobbyHubClient` handlers registered in `LobbyServices`.
- `Client/Services/LobbyServices.cs` owns the `HubConnection` to `/lobby`, registers client-side handlers for every `ILobbyHubClient` method (typed on `GameDto`/`PlayerDto`), and exposes methods matching `ILobbyHubServer` for the app loop to call. Its `StartGameAsync` busy-waits on `Players.Count < MinimumPlayers` with an empty loop and no reactive update path - a pre-existing latent bug (nothing ever mutates `CurrentGame` after it's set), left as-is; flagged here rather than silently fixed since it can freeze the console app after creating/joining a game with too few players.
- `Client/UI/ConsoleUI.cs` / `ConsoleAnimator.cs` are the only I/O helpers; all user-facing output should go through them rather than raw `Console.Write`.

### Shared models
`Shared/Models/*` are plain mutable classes (`Game`, `Player`, `Round`, `Skill`, `Company`, `Consultant`, `Tender`, `Training`) - the domain model, used server-side and as the source `Mapper.ToDto` reads from. `Shared/Models/Dtos/*` (`GameDto`, `PlayerDto`, `Mapper`) are the actual wire contracts sent to clients. `BaseModel` gives entities a string GUID `Id`. `Shared/Models/Requests/*` are the hub-call command payloads (`CreateGameCommand`, `JoinGameCommand`, `CreatePlayerCommand`).
