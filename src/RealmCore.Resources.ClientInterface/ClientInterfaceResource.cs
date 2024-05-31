using SlipeServer.Server.Resources;

namespace RealmCore.Resources.ClientInterface;

internal class ClientInterfaceResource : Resource
{
    internal ClientInterfaceResource(MtaServer server) : base(server, server.RootElement, "ClientInterface") { }
}
