using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server;

namespace Realm.Resources.Assets;

internal class AssetsResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["assets.lua"] = ResourceFiles.Assets,
    };

    internal AssetsResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "Assets")
    {
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}
