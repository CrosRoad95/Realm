using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;

namespace RealmCore.Resources.Admin;

internal class AdminResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["debugDraw.lua"] = ResourceFiles.DebugDraw,
        ["admin.lua"] = ResourceFiles.Admin,
        ["tools/entities.lua"] = ResourceFiles.ToolEntities,
        ["tools/components.lua"] = ResourceFiles.ToolComponents,
    };

    internal AdminResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "Admin")
    {
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}
