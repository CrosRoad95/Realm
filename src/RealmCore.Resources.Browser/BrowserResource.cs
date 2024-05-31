namespace RealmCore.Resources.Browser;

internal class BrowserResource : Resource
{
    internal BrowserResource(MtaServer server) : base(server, server.RootElement, "Browser") { }
}