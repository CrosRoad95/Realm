using Grpc.Core;
using Microsoft.Extensions.Options;
using Realm.Configuration;
using Realm.DiscordBot.Stubs;
using Realm.Module.Grpc.Options;

namespace Realm.DiscordBot;

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
