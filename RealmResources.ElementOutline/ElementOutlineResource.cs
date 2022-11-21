using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server;

namespace RealmResources.ElementOutline;

internal class ElementOutlineResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["outline.lua"] = ResourceFiles.Outline,
    };

    internal ElementOutlineResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "ElementOutline")
    {
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}
