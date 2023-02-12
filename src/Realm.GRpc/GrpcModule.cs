namespace Realm.Module.Grpc;

internal sealed class GrpcModule : IModule
{
    private Server? _grpcServer;
    private ILogger<GrpcModule> _logger;
    public GrpcModule(ILogger<GrpcModule> logger, GreeterServiceStub greeterServiceStub,
        DiscordHandshakeServiceStub discordHandshakeServiceStub, DiscordStatusChannelServiceStub discordStatusChannelServiceStub,
        IRealmConfigurationProvider realmConfigurationProvider)
    {
        _logger = logger;
        var configuration = realmConfigurationProvider.GetRequired<GrpcConfiguration>("Grpc");
        _grpcServer = new Server
        {
            Services =
            {
                Greet.Greeter.BindService(greeterServiceStub),
                Discord.Handshake.BindService(discordHandshakeServiceStub),
                Discord.StatusChannel.BindService(discordStatusChannelServiceStub),
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
