using Realm.Configuration;
using Realm.Interfaces.Extend;
using Realm.Module.Discord.Stubs;
using Realm.Module.Grpc;

namespace Realm.Module.Discord;

internal class DiscordModule : IModule
{
    private readonly ILogger<DiscordModule> _logger;
    private readonly Server? _grpcServer;
    private readonly IDiscordStatusChannelUpdateHandler? _discordStatusChannelUpdateHandler;
    private readonly IDiscordConnectAccountHandler? _discordConnectAccountHandler;
    private readonly IDiscordPrivateMessageReceived? _discordPrivateMessageReceived;

    public DiscordModule(ILogger<DiscordModule> logger, IDiscordService grpcDiscord,
        DiscordHandshakeServiceStub discordHandshakeServiceStub, DiscordStatusChannelServiceStub discordStatusChannelServiceStub,
        DiscordConnectAccountChannelStub discordConnectAccountChannelStub,
        DiscordPrivateMessagesChannelsStub discordPrivateMessagesChannelsStub,
        IRealmConfigurationProvider realmConfigurationProvider,
        IDiscordStatusChannelUpdateHandler? discordStatusChannelUpdateHandler = null,
        IDiscordConnectAccountHandler? discordConnectAccountHandler = null,
        IDiscordPrivateMessageReceived? discordPrivateMessageReceived = null
        )
    {
        grpcDiscord.UpdateStatusChannel = HandleUpdateStatusChannel;
        grpcDiscord.TryConnectAccountChannel = HandleTryConnectAccountChannel;
        grpcDiscord.PrivateMessageReceived = HandlePrivateMessageReceived;
        _logger = logger;
        _discordStatusChannelUpdateHandler = discordStatusChannelUpdateHandler;
        _discordConnectAccountHandler = discordConnectAccountHandler;
        _discordPrivateMessageReceived = discordPrivateMessageReceived;

        var configuration = realmConfigurationProvider.GetRequired<GrpcConfiguration>("Grpc");
        _grpcServer = new Server
        {
            Services =
            {
                Handshake.BindService(discordHandshakeServiceStub),
                StatusChannel.BindService(discordStatusChannelServiceStub),
                ConnectAccountChannel.BindService(discordConnectAccountChannelStub),
                PrivateMessagesChannels.BindService(discordPrivateMessagesChannelsStub),
            },
            Ports =
            {
                new ServerPort(configuration.Host, configuration.Port, ServerCredentials.Insecure)
            },
        };

        _grpcServer.Start();
        _logger.LogInformation("Started grpc server.");
    }

    public async Task<string> HandleUpdateStatusChannel(CancellationToken cancellationToken)
    {
        if (_discordStatusChannelUpdateHandler == null)
            return "Discord status channel update handler is not configured properly.";

        return await _discordStatusChannelUpdateHandler.HandleStatusUpdate(cancellationToken);
    }

    public async Task<TryConnectResponse> HandleTryConnectAccountChannel(string code, ulong userId, CancellationToken cancellationToken)
    {
        if (_discordConnectAccountHandler == null)
            return new TryConnectResponse
            {
                success = false,
                message = "Discord connection handles is not configured properly."
            };

        return await _discordConnectAccountHandler.HandleConnectAccount(code, userId, cancellationToken);
    }

    public void HandlePrivateMessageReceived(ulong userId, ulong messageId, string content, CancellationToken cancellationToken)
    {
        if(_discordPrivateMessageReceived != null)
            _discordPrivateMessageReceived.HandlePrivateMessage(userId, messageId, content);
    }
}