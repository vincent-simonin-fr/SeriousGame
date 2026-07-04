using Client.State;
using Client.UI;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Client.Services;

public class HubConnectionManager
{
    private readonly string _baseHubUrl;
    private readonly HubConnection _lobbyHubConnection;
    private readonly HubConnection _gameHubConnection;
    private readonly HubConnection _chatHubConnection;

    public HubConnectionManager(string baseHubUrl)
    {
        _baseHubUrl = baseHubUrl;

        _lobbyHubConnection = new HubConnectionBuilder()
            .WithUrl($"{_baseHubUrl}/lobby", options =>
            {
                options.Transports = HttpTransportType.WebSockets;
                options.HttpMessageHandlerFactory = _ => new HttpClientHandler
                {
                    // TODO : Secure fail
                    ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
                    // Dev only: accepter un certificat auto-signé (dangerous)
                    //ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            })
            .WithAutomaticReconnect()
            .Build();
        
        _gameHubConnection = new HubConnectionBuilder()
            .WithUrl($"{_baseHubUrl}/game", HttpTransportType.WebSockets)
            .WithAutomaticReconnect()
            .Build();
        
        _chatHubConnection = new HubConnectionBuilder()
            .WithUrl($"{_baseHubUrl}/chat", HttpTransportType.WebSockets)
            .WithAutomaticReconnect()
            .Build();
        
        RegisterLobbyEvents();
    }

    private void RegisterLobbyEvents()
    {
        _lobbyHubConnection.On<string>("Notify", msg =>
        {
            ConsoleUI.WriteInfo($"[From the Server] {msg}");
        });

        _lobbyHubConnection.Reconnecting += error =>
        {
            ConsoleUI.WriteError("⚠️ Reconnecting...");
            return Task.CompletedTask;
        };

        _lobbyHubConnection.Reconnected += async id =>
        {
            // Update ConnectionId
            await _lobbyHubConnection.InvokeAsync("UpdatePlayerConnectionId", ClientIdentity.Id);
            ConsoleUI.WriteInfo("🔗 Reconnected !");
            await Task.CompletedTask;
        };
    }

    public async Task<bool> ConnectAsync()
    {
        try
        {
            await _lobbyHubConnection.StartAsync();
            ConsoleUI.WriteInfo($"Connected to the Hub : {_baseHubUrl}");
            return true;
        }
        catch
        {
            ConsoleUI.WriteError($"Failed to connect to Hub : {_baseHubUrl}. " +
                                 $"If the problem persists, please contact an administrator.");
            return false;
        }
    }

    public async Task SendAsync(string methodName, params object[] args)
    {
        if (_chatHubConnection.State != HubConnectionState.Connected)
        {
            ConsoleUI.WriteError($"Cannot invoke '{methodName}': connection state = {_lobbyHubConnection.State}");
            return;
        }
        
        try
        {
            await _chatHubConnection.InvokeCoreAsync(methodName, args);
        }
        catch (HubException hex)
        {
            ConsoleUI.WriteError($"HubException while invoking '{methodName}': {hex.Message}");
        }
        catch (InvalidOperationException iox)
        {
            ConsoleUI.WriteError($"Invalid operation during '{methodName}': {iox.Message}");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Unexpected error while sending '{methodName}': {ex.Message}");
        }
    }
    
    public async Task<T?> SendAsync<T>(string methodName, params object[] args)
    {
        if (_chatHubConnection.State != HubConnectionState.Connected)
        {
            ConsoleUI.WriteError($"Cannot invoke '{methodName}': connection state = {_lobbyHubConnection.State}");
            return default;
        }
        
        try
        {
            return await _chatHubConnection.InvokeCoreAsync<T>(methodName, args);
        }
        catch (HubException hex)
        {
            ConsoleUI.WriteError($"HubException while invoking '{methodName}': {hex.Message}");
        }
        catch (InvalidOperationException iox)
        {
            ConsoleUI.WriteError($"Invalid operation during '{methodName}': {iox.Message}");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Unexpected error while sending '{methodName}': {ex.Message}");
        }

        return default;
    }

    public async Task DisconnectAsync()
    {
        await _lobbyHubConnection.StopAsync();
        await _gameHubConnection.StopAsync();
        await _chatHubConnection.StopAsync();
        ConsoleUI.WriteInfo("You have been disconnected!.");
    }
}