namespace RealmCore.Module.Discord.Interfaces;

public struct TryConnectResponse
{
    public bool success;
    public string message;
}

public delegate Task<string> UpdateStatusChannel(CancellationToken cancellationToken);
public delegate Task<TryConnectResponse> TryConnectUserChannel(string code, ulong userId, CancellationToken cancellationToken);
public delegate Task PrivateMessageReceived(ulong userId, ulong messageId, string content, CancellationToken cancellationToken);
public delegate Task TextBasedMessageReceived(ulong userId, ulong channelId, string command, CancellationToken cancellationToken);

public interface IDiscordService
{
    internal UpdateStatusChannel? UpdateStatusChannel { get; set; }
    internal TryConnectUserChannel? TryConnectUserChannel { get; set; }
    internal PrivateMessageReceived? PrivateMessageReceived { get; set; }
    internal TextBasedMessageReceived? TextBasedCommandReceived { get; set; }

    void AddTextBasedCommandHandler(ulong channelId, string command, Func<ulong, string, Task> callback);
    Task HandleTextBasedCommand(ulong userId, ulong channelId, string command);
    Task<ulong> SendFile(ulong channelId, Stream file, string fileName, string message);
    Task<ulong> SendMessage(ulong channelId, string message);
    Task<ulong> SendMessageToUser(ulong userId, string message);
}
