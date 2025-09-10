using Microsoft.AspNetCore.SignalR.Client;
using Monolith.HttpApi.Common.Interfaces;
using Monolith.Notifications;

namespace Monolith.Core;

public class SignalRClient(
    ITokenProvider tokenService,
    IWebSettings webSettings) : IAsyncDisposable
{
    private HubConnection? _hubConnection;

    //public event Action? OnMessageReceived;

    public async Task<bool> ConnectAsync()
    {
        if (_hubConnection != null) return true;

        var hubUrl = webSettings.SignalRHub;

        Task<string?> AccessTokenProvider() => tokenService.GetAccessTokenAsync();

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = AccessTokenProvider; // Provide the access token
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                options.SkipNegotiation = true;
            })
            .WithAutomaticReconnect()
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Information); // Set log level to debug
            })
            .Build();

        try
        {
            await _hubConnection.StartAsync();

            Console.WriteLine($"SignalR connected");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR error: {ex.Message}");

            return false;
        }

        return true;
    }

    public void On<T>(Action<T> action)
        where T : class, INotificationMessage
    {
        // Listen for incoming messages
        _hubConnection?.On<T>(typeof(T).Name, action.Invoke);
    }

    public async Task SendMessageAsync(string user, string message)
    {
        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("SendMessage", user, message);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
            GC.SuppressFinalize(this);
        }
    }
}
