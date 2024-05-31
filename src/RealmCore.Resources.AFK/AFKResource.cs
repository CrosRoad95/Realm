using SlipeServer.Server;
using SlipeServer.Server.Resources;

namespace RealmCore.Resources.AFK;

internal class AFKResource : Resource
{
    internal AFKResource(MtaServer server) : base(server, server.RootElement, "AFK") { }
}