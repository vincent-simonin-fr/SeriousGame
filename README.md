# Why So Serious

This repository contains two applications developed using .NET 10:
1. **WebAPI**: A backend API for handling data and WebSocket connections.
2. **CLI**: A command-line application that communicates with the WebAPI using SignalR.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Cloning the Repository](#cloning-the-repository)
- [Running the WebAPI](#running-the-webapi)
- [Running the CLI](#running-the-cli)
- [Publishing the Applications](#publishing-the-applications)
- [Configuration](#configuration)
- [Setting up Docker](#setting-up-docker)
- [ASCII Art](#ascii-art)
- [Troubleshooting](#troubleshooting)

## Prerequisites
Before you begin, ensure you have the following installed on your machine:
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/get-started)
- [Git](https://git-scm.com/)

## Cloning the Repository
To clone the repository, run the following command:

```bash
git clone 
cd DI1-P1
```

## Design
### Sequence Diagram Create game
```scss
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

### Sequence Diagram Join game
```scss
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
### Sequence Diagram OnDisconnectedAsync
```scss
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
  │     RemovePlayerByConnectionId(connectionId)
  ├──────────────────────────────────────► PlayerService
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
## Class Diagram
```scss
                         ┌───────────────────────────────┐
                         │         GameFlowService       │
                         │───────────────────────────────│
                         │ + CreateGame(cmd, connId)     │
                         │ + JoinGame(cmd, connId)       │
                         │ + RemovePlayer(connId)        │ 
                         │ + TryStartGame(game)          │
                         │ + NotifyGameState(game)       │
                         └───────────────▲───────────────┘
                                         │ uses
┌────────────────────────────┐           │             ┌───────────────────────────┐
│        GameService         │◄──────────┘             │       PlayerService       │
│────────────────────────────│                         │───────────────────────────│
│ + CreateGame(name, owner)  │                         │ + CreatePlayer(...)       │
│ + JoinGame(id, player)     │                         │ + GetPlayerById(id)       │
│ + GetGame(id)              │                         │ + RemovePlayer(connId)    │
│ + UpdateGameState()        │                         │ + RemoveFromGame(connId)  │
└────────────────────────────┘                         └──────────▲────────────────┘
                                                                  │
                                                ┌─────────────────┴──────────────┐
                                                │      Domain Models             │
                                                │────────────────────────────────│
                                                │ Game, Player, GameState, etc.  │
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
              │         GameFlowService     │
              └─────────────────────────────┘
```

## Running the WebAPI

Navigate to the Server project directory:

```bash
cd src/Server
```

Update the appsettings.json file if necessary (default configuration connects to PostgreSQL via localhost).

Run the WebAPI:

```bash
dotnet run --launch-profile https
```

The WebAPI will start running on https://localhost:7032 (as per your configuration).

## Running the CLI

Navigate to the Client project directory:

```bash
cd src/Client
```

Ensure the appsettings.json file contains the correct API and WebSocket server configuration. The default points to the WebAPI running locally.

Run the CLI application:

```bash
dotnet run
```

This will initialize the TUI (Terminal User Interface) which connects to the WebAPI and WebSocket server.

## Publishing the Applications

### Publishing the WebAPI

To publish the WebAPI to a folder or server, run the following command in the Server project directory:

```bash
dotnet publish -c Release -o ./publish
```

This will generate the compiled API in the ./publish folder, ready for deployment.

### Publishing the CLI

Similarly, to publish the CLI application, run the following command in the Client project directory:

```bash
dotnet publish -c Release -o ./publish
```

This will generate the compiled CLI in the ./publish folder.

## Configuration

### WebAPI Configuration

The WebAPI uses appsettings.json for configuration. The default configuration can be found in Server/appsettings.json. Key settings include:

Database Configuration:

```json
"Database": {
  "Host": "127.0.0.1",
  "Port": "5432",
  "Name": "wss_dev",
  "User": "wss",
  "Pass": "WSS"
}
```

### CLI Configuration

The CLI application uses Client/appsettings.json to configure the API and WebSocket server connections:

- WebSocket Server Configuration:

```json
"WebSocketServer": {
  "Scheme": "wss",
  "Domain": "localhost",
  "Port": "7032"
}
```

## Setting up Docker (PostgreSQL and PgAdmin)

The project includes a Docker Compose file. To start, navigate to the project root and run:

```bash
docker-compose up -d
```

## Troubleshooting
- Several players can have the same login
- No certificateÒ

## ASCII Art
If you want to write in ASCII, click [here!](https://patorjk.com/software/taag/#p=display&f=Graffiti&t=Type+Something+&x=none&v=4&h=4&w=80&we=false)