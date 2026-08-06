namespace PvZWSTools_WPF.Services;

public interface IConnectionService
{
    bool IsConnected { get; }

    event EventHandler<bool> ConnectionStateChanged;

    event EventHandler<string> ConnectionError;

    event EventHandler<string> MessageReceived;

    Task ConnectAsync(string address, CancellationToken cancellationToken = default);

    void Disconnect();

    Task SendAsync(string message);
}
