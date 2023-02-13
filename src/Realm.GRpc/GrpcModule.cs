using Discord;

namespace Realm.Module.Grpc;

internal sealed class GrpcModule : IModule
{
    private readonly Server? _grpcServer;
    private readonly ILogger<GrpcModule> _logger;

    public GrpcModule(ILogger<GrpcModule> logger, GreeterServiceStub greeterServiceStub,
        DiscordHandshakeServiceStub discordHandshakeServiceStub, DiscordStatusChannelServiceStub discordStatusChannelServiceStub,
        DiscordConnectAccountChannelStub discordConnectAccountChannelStub,
        IRealmConfigurationProvider realmConfigurationProvider)
    {
        _logger = logger;
        var configuration = realmConfigurationProvider.GetRequired<GrpcConfiguration>("Grpc");
        _grpcServer = new Server
        {
            Services =
            {
                Greeter.BindService(greeterServiceStub),
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
}
