using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server;

namespace RealmCore.Resources.ClientInterface;

internal class ClientInterfaceResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["debugging.lua"] = ResourceFiles.Debugging,
        ["utility.lua"] = ResourceFiles.Utility,
        ["player.lua"] = ResourceFiles.Player,
        ["focusable.lua"] = ResourceFiles.Focusable,
    };

    internal ClientInterfaceResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "ClientInterface")
    {
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}
