using Grpc.Core;
using Microsoft.Extensions.Options;
using RealmCore.DiscordBot.Stubs;
using RealmCore.Module.Grpc.Options;

namespace RealmCore.DiscordBot;

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
