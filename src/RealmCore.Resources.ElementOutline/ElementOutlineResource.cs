using SlipeServer.Server.Resources;
using SlipeServer.Server;

namespace RealmCore.Resources.ElementOutline;

internal class ElementOutlineResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["outline.lua"] = ResourceFiles.Outline,
    };

    internal ElementOutlineResource(MtaServer server)
        : base(server, server.RootElement, "ElementOutline")
    {
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}
