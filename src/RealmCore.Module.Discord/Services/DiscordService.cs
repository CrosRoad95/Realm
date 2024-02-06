namespace RealmCore.Module.Discord.Services;

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
    Task HandleTextBasedCommand(ulong userId, ulong channelId, string command, CancellationToken cancellationToken = default);
    Task<ulong> SendFile(ulong channelId, Stream file, string fileName, string message, CancellationToken cancellationToken = default);
    Task<ulong> SendMessage(ulong channelId, string message, CancellationToken cancellationToken = default);
    Task<ulong> SendMessageToUser(ulong userId, string message, CancellationToken cancellationToken = default);
}

internal class DiscordService : IDiscordService
{
    private readonly Messaging.MessagingClient _messagingClient;

    public DiscordService(GrpcChannel grpcChannel)
    {
        _messagingClient = new(grpcChannel);
    }

    public UpdateStatusChannel? UpdateStatusChannel { get; set; }
    public TryConnectUserChannel? TryConnectUserChannel { get; set; }
    public PrivateMessageReceived? PrivateMessageReceived { get; set; }
    public TextBasedMessageReceived? TextBasedCommandReceived { get; set; }

    public async Task<ulong> SendMessage(ulong channelId, string message, CancellationToken cancellationToken = default)
    {
        var response = await _messagingClient.SendMessageAsync(new SendMessageRequest
        {
            ChannelId = channelId,
            Message = message,
        }, cancellationToken: cancellationToken);

        if (response.Success)
            return response.MessageId;

        throw new Exception("Failed to send message");
    }

    public async Task<ulong> SendFile(ulong channelId, Stream file, string fileName, string message, CancellationToken cancellationToken = default)
    {
        var byteString = await ByteString.FromStreamAsync(file, cancellationToken);
        var response = await _messagingClient.SendFileAsync(new SendFileRequest
        {
            File = byteString,
            FileName = fileName,
            ChannelId = channelId,
            Message = message,
        }, cancellationToken: cancellationToken);

        if (response.Success)
            return response.MessageId;

        throw new Exception("Failed to send file");
    }

    public async Task<ulong> SendMessageToUser(ulong userId, string message, CancellationToken cancellationToken = default)
    {
        var response = await _messagingClient.SendMessageToUserAsync(new SendMessageToUserRequest
        {
            UserId = userId,
            Message = message,
        }, cancellationToken: cancellationToken);

        if (response.Success)
            return response.MessageId;

        throw new Exception("Failed to send message");
    }

    Dictionary<ulong, Dictionary<string, Func<ulong, string, Task>>> _textBasedCommandHandlers = new();

    public async Task HandleTextBasedCommand(ulong userId, ulong channelId, string command, CancellationToken cancellationToken = default)
    {
        if (_textBasedCommandHandlers.TryGetValue(channelId, out var channelCommands))
        {
            var splitted = command.Split(' ', 2);
            if (channelCommands.TryGetValue(splitted[0], out var commandHandler))
            {
                await commandHandler(userId, command[splitted[0].Length..]);
            }
        }
    }

    public void AddTextBasedCommandHandler(ulong channelId, string command, Func<ulong, string, Task> callback)
    {
        if (!_textBasedCommandHandlers.ContainsKey(channelId))
        {
            _textBasedCommandHandlers[channelId] = new();
        }

        _textBasedCommandHandlers[channelId][command] = callback;
    }
}
