# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

Build/run from repo root (`SeriousGame.sln`) or per-project.

```bash
dotnet build                              # build whole solution
dotnet run --project Server --launch-profile https   # run WebAPI (SignalR server), https://localhost:7289
dotnet run --project Client                           # run CLI/TUI client
```

- No test project exists in this repo yet.
- Server config: `Server/appsettings.json` / `Server/appsettings.Development.json`. Client config: `Client/appsettings.json` (embedded resource, copied to output).
- Client persists identity to `client_id_<guid>.txt` next to the binary (gitignored via `client_id*.txt`).
- Dockerfiles exist per-project (`Client/Dockerfile`, `Server/Dockerfile`) but there is no docker-compose file in the repo despite README references to one — don't assume it exists.

## Architecture

Three-project .NET 9 solution: `Server` (ASP.NET Core + SignalR hubs, `Sdk="Microsoft.NET.Sdk.Web"`), `Client` (console/TUI app using `SignalR.Client`), `Shared` (POCOs/DTOs/abstractions referenced by both). Server and Client only depend on Shared — never reference each other directly; cross-process contracts live in `Shared/Abstractions` (`ILobbyHubClient`, `ILobbyHubServer`) as the source of truth for hub method names/signatures.

### Server state model
All state is in-memory, held by the singleton `Server/AppMemory.cs` (`ICollection<Player>`, `ICollection<Game>`, plus a static `Skills` seed list). No database is wired up despite the README describing PostgreSQL config — treat that section of the README as aspirational/stale. `GameService` and `PlayerService` are scoped services that mutate `AppMemory` directly (no repository layer).

### Two parallel game-flow implementations (known duplication, not yet consolidated)
- `Server/Hubs/LobbyHub.cs` (`Hub<ILobbyHubClient>`, mapped at `/lobby`) implements `ILobbyHubServer` directly, calling `GameService`/`PlayerService` and pushing notifications to `Clients.*` inline. This is the hub actually wired up in `Program.cs` and used by the Client (`Client/Services/LobbyServices.cs` invokes lobby methods by `nameof(ILobbyHubServer.*)`/`nameof(ILobbyHubClient.*)`).
- `Server/Services/GameFlowSerice.cs` (`GameFlowService`) re-implements the same create/join/disconnect flow using `IHubContext<LobbyHub, ILobbyHubClient>`, returning `GameDto` instead of raw `Game`. **It is not registered in DI in `Program.cs`** and is only referenced as an unused constructor parameter on `LobbyHub` — if you touch lobby flow logic, check whether the intent is to finish migrating `LobbyHub` onto `GameFlowService` or to delete the dead one, rather than editing both.
- `Server/Hubs/GameHub.cs` (mapped at `/game`) is a mostly-separate, less-developed hub for in-game actions; it duplicates game-join logic with plain `SendAsync("EventName", ...)` calls instead of the strongly-typed `Hub<TClient>` pattern the lobby uses.
- `Shared/Models/Dtos/GameDto.cs`'s `Projection` expression is an empty object initializer — `GameDto.FromEntity`/`Projection` currently produce blank DTOs; don't assume fields are populated without checking first.

### Client structure
- `Client/Core/GameClientApp.cs` drives the console loop (connect → identify nickname → main menu → create/join game).
- `Client/Core/ClientIdentity.cs` generates/persists a per-run GUID-based player id to a local file; `ClientMemory.cs` holds mutable client-side game state (`Games` list, `CurrentGame`) populated by the `ILobbyHubClient` handlers registered in `LobbyServices`.
- `Client/Services/LobbyServices.cs` owns the `HubConnection` to `/lobby`, registers client-side handlers for every `ILobbyHubClient` method, and exposes methods matching `ILobbyHubServer` for the app loop to call.
- `Client/Services/HubClientManager.cs` (`HubConnectionManager`) is a separate, more generic multi-hub (lobby/game/chat) connection manager that is not currently used by `GameClientApp`/`LobbyServices` — those instantiate `LobbyServices` directly with its own `HubConnection`. `Client/Services/GameServices.cs` is an empty stub; `Client/Services/yo.cs` defines an unrelated, unused in-memory `ILobbyService`/`LobbyService` (room concept keyed by `Guid`, separate from `Shared.Models.Game`) — don't confuse it with the real lobby flow.
- `Client/UI/ConsoleUI.cs` / `ConsoleAnimator.cs` are the only I/O helpers; all user-facing output should go through them rather than raw `Console.Write`.

### Shared models
`Shared/Models/*` are plain mutable classes (`Game`, `Player`, `Round`, `Skill`, `Company`, `Consultant`, `Tender`, `Training`) shared verbatim between Server and Client over the SignalR wire (no separate wire-format DTOs except the still-empty `GameDto`). `BaseModel` gives entities a string GUID `Id`. `Shared/Models/Requests/*` are the hub-call command payloads (`CreateGameCommand`, `JoinGameCommand`, `CreatePlayerCommand`).
