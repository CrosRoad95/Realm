using SlipeServer.Server;
using SlipeServer.Server.Resources;

namespace RealmCore.Resources.Admin;

internal class AdminResource : Resource
{
    internal AdminResource(MtaServer server) : base(server, server.RootElement, "Admin") { }
}
