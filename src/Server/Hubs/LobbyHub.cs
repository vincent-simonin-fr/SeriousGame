using Microsoft.AspNetCore.SignalR;
using Server.Application.Abstractions;
using Shared.Abstractions;
using Shared.Models.Dtos;

namespace Server.Hubs;

/// <summary>
/// Thin SignalR adapter: parses each call and delegates all business logic to ILobbyFlowService.
/// </summary>
public sealed class LobbyHub : Hub<ILobbyHubClient>, ILobbyHubServer
{
    private readonly ILobbyFlowService _lobbyFlowService;

    public LobbyHub(ILobbyFlowService lobbyFlowService)
    {
        _lobbyFlowService = lobbyFlowService;
    }

    public override async Task OnConnectedAsync()
    {
        await _lobbyFlowService.NotifyLobbyOnConnection(Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public async Task IdentifyNewPlayer(CreatePlayerCommand command)
    {
        await _lobbyFlowService.IdentifyNewPlayer(command, Context.ConnectionId);
        await Clients.Caller.Notify($"Hi {command.Nickname}, welcome to the game!");
    }

    public async Task UpdatePlayerConnectionId(string clientId)
    {
        await _lobbyFlowService.UpdatePlayerConnectionId(clientId, Context.ConnectionId);
    }

    public async Task SendMessageToAllPlayers(string message)
    {
        await Clients.Others.Notify($"{message}");
    }

    public Task<GameDto> CreateGame(CreateGameCommand command)
    {
        return _lobbyFlowService.CreateGame(command, Context.ConnectionId);
    }

    public Task<GameDto?> JoinGame(JoinGameCommand command)
    {
        return _lobbyFlowService.JoinGame(command, Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        await _lobbyFlowService.HandlePlayerDisconnect(Context.ConnectionId);
    }
}
