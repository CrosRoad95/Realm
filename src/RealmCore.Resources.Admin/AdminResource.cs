using SlipeServer.Server;
using SlipeServer.Server.Resources;

namespace RealmCore.Resources.Admin;

internal class AdminResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["debugDraw.lua"] = ResourceFiles.DebugDraw,
        ["admin.lua"] = ResourceFiles.Admin,
        ["tools/elements.lua"] = ResourceFiles.ToolElements,
        ["tools/spawnMarkers.lua"] = ResourceFiles.ToolSpawnMarkers,
    };

    internal AdminResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "Admin")
    {
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}
