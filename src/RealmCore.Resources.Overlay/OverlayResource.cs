using SlipeServer.Server;
using SlipeServer.Server.Resources;

namespace RealmCore.Resources.Overlay;

internal class OverlayResource : Resource
{
    internal OverlayResource(MtaServer server) : base(server, server.RootElement, "Overlay") { }
}
