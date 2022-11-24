using SlipeServer.Server.Elements;
using SlipeServer.Server;
using SlipeServer.Server.Resources;

namespace Realm.Resources.AFK;

internal class AFKResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["afk.lua"] = ResourceFiles.afk,
    };

    internal AFKResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "AFK")
    {
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}