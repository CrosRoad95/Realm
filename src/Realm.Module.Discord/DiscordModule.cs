using Greet;
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

    public DiscordModule(ILogger<DiscordModule> logger, IDiscordService grpcDiscord,
        DiscordHandshakeServiceStub discordHandshakeServiceStub, DiscordStatusChannelServiceStub discordStatusChannelServiceStub,
        DiscordConnectAccountChannelStub discordConnectAccountChannelStub,
        IRealmConfigurationProvider realmConfigurationProvider,
        IDiscordStatusChannelUpdateHandler? discordStatusChannelUpdateHandler = null,
        IDiscordConnectAccountHandler? discordConnectAccountHandler = null
        )
    {
        grpcDiscord.UpdateStatusChannel = HandleUpdateStatusChannel;
        grpcDiscord.TryConnectAccountChannel = HandleTryConnectAccountChannel;
        _logger = logger;
        _discordStatusChannelUpdateHandler = discordStatusChannelUpdateHandler;
        _discordConnectAccountHandler = discordConnectAccountHandler;

        var configuration = realmConfigurationProvider.GetRequired<GrpcConfiguration>("Grpc");
        _grpcServer = new Server
        {
            Services =
            {
                Handshake.BindService(discordHandshakeServiceStub),
                StatusChannel.BindService(discordStatusChannelServiceStub),
                ConnectAccountChannel.BindService(discordConnectAccountChannelStub),
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
}