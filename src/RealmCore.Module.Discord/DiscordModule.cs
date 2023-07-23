using RealmCore.Interfaces.Extend;

namespace RealmCore.Module.Discord;

internal class DiscordModule : IModule
{
    private readonly ILogger<DiscordModule> _logger;
    private readonly IDiscordStatusChannelUpdateHandler? _discordStatusChannelUpdateHandler;
    private readonly IDiscordConnectUserHandler? _discordConnectUserHandler;
    private readonly IDiscordPrivateMessageReceived? _discordPrivateMessageReceived;
    private readonly IDiscordTextBasedCommandHandler? _discordTextBasedCommandHandler;

    public DiscordModule(ILogger<DiscordModule> logger, IDiscordService grpcDiscord,
        IDiscordStatusChannelUpdateHandler? discordStatusChannelUpdateHandler = null,
        IDiscordConnectUserHandler? discordConnectUserHandler = null,
        IDiscordPrivateMessageReceived? discordPrivateMessageReceived = null,
        IDiscordTextBasedCommandHandler? discordTextBasedCommandHandler = null
        )
    {
        grpcDiscord.UpdateStatusChannel = HandleUpdateStatusChannel;
        grpcDiscord.TryConnectUserChannel = HandleTryConnectUserChannel;
        grpcDiscord.PrivateMessageReceived = HandlePrivateMessageReceived;
        grpcDiscord.TextBasedCommandReceived = HandleTextBasedMessageReceived;
        _logger = logger;
        _discordStatusChannelUpdateHandler = discordStatusChannelUpdateHandler;
        _discordConnectUserHandler = discordConnectUserHandler;
        _discordPrivateMessageReceived = discordPrivateMessageReceived;
        _discordTextBasedCommandHandler = discordTextBasedCommandHandler;

        _logger.LogInformation("{module} module loaded", "Discord");
    }

    public async Task<string> HandleUpdateStatusChannel(CancellationToken cancellationToken)
    {
        if (_discordStatusChannelUpdateHandler == null)
            return "Discord status channel update handler is not configured properly.";

        return await _discordStatusChannelUpdateHandler.HandleStatusUpdate(cancellationToken);
    }

    public async Task<TryConnectResponse> HandleTryConnectUserChannel(string code, ulong userId, CancellationToken cancellationToken)
    {
        if (_discordConnectUserHandler == null)
            return new TryConnectResponse
            {
                success = false,
                message = "Discord connection handles is not configured properly."
            };

        return await _discordConnectUserHandler.HandleConnectUser(code, userId, cancellationToken);
    }

    public async Task HandlePrivateMessageReceived(ulong userId, ulong messageId, string content, CancellationToken cancellationToken)
    {
        if (_discordPrivateMessageReceived != null)
            await _discordPrivateMessageReceived.HandlePrivateMessage(userId, messageId, content);
    }

    public async Task HandleTextBasedMessageReceived(ulong userId, ulong messageId, string command, CancellationToken cancellationToken)
    {
        if (_discordTextBasedCommandHandler != null)
            await _discordTextBasedCommandHandler.HandleTextCommand(userId, messageId, command);
    }
}