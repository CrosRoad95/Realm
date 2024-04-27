namespace RealmCore.Discord.Integration;

internal sealed class GrpcServer
{
    private readonly Server _grpcServer;
    public GrpcServer(IOptions<GrpcOptions> grpcOptions, MessagingServiceStub messagingServiceStub)
    {
        _grpcServer = new Server
        {
            Services =
            {
                Messaging.BindService(messagingServiceStub),
            },
            Ports =
            {
                new ServerPort(grpcOptions.Value.Host, grpcOptions.Value.Port, ServerCredentials.Insecure)
            },
        };
    }

    public void Start()
    {
        _grpcServer.Start();
    }
}
