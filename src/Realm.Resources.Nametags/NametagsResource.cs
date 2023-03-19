using SlipeServer.Server.Elements;
using SlipeServer.Server;
using SlipeServer.Server.Resources;

namespace Realm.Resources.Nametags;

internal class NametagsResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["nametags.lua"] = ResourceFiles.nametags,
    };

    internal NametagsResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "Nametags")
    {
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}