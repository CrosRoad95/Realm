using SlipeServer.Server.Resources;
using SlipeServer.Server;

namespace RealmCore.Resources.ElementOutline;

internal class ElementOutlineResource : Resource
{
    internal ElementOutlineResource(MtaServer server) : base(server, server.RootElement, "ElementOutline") { }
}
