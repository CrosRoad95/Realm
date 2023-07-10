using Grpc.Core;
using RealmCore.Discord.Integration.Stubs;
using RealmCore.Module.Grpc.Options;

namespace RealmCore.Discord.Integration;

internal class GrpcServer
{
    private readonly Server? _grpcServer;
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

        _grpcServer.Start();
    }
}
