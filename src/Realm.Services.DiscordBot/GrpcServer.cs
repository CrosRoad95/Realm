using Grpc.Core;
using Realm.Configuration;
using Realm.DiscordBot.Stubs;
using Realm.Module.Grpc;

namespace Realm.DiscordBot;

internal class GrpcServer
{
    private readonly Server? _grpcServer;
    public GrpcServer(IRealmConfigurationProvider realmConfigurationProvider, MessagingServiceStub messagingServiceStub)
    {
        var configuration = realmConfigurationProvider.GetRequired<GrpcConfiguration>("Grpc");
        _grpcServer = new Server
        {
            Services =
            {
                Messaging.BindService(messagingServiceStub),
            },
            Ports =
            {
                new ServerPort(configuration.Host, configuration.Port, ServerCredentials.Insecure)
            },
        };

        _grpcServer.Start();
    }
}
