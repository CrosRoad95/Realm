namespace Realm.Module.Discord.Interfaces;

public struct TryConnectResponse
{
    public bool success;
    public string message;
}

public delegate Task<string> UpdateStatusChannel(CancellationToken cancellationToken);
public delegate Task<TryConnectResponse> TryConnectAccountChannel(string code, ulong userId, CancellationToken cancellationToken);
public delegate void PrivateMessageReceived(ulong userId, ulong messageId, string content, CancellationToken cancellationToken);

public interface IDiscordService
{
    UpdateStatusChannel? UpdateStatusChannel { get; set; }
    TryConnectAccountChannel? TryConnectAccountChannel { get; set; }
    PrivateMessageReceived? PrivateMessageReceived { get; set; }

    Task<ulong> SendFile(ulong channelId, Stream file, string fileName, string message);
    Task<ulong> SendMessage(ulong channelId, string message);
    Task<ulong> SendMessageToUser(ulong userId, string message);
}
