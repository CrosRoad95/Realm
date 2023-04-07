using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;

namespace RealmCore.Resources.AdminTools;

internal class AdminToolsResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["debugDraw.lua"] = ResourceFiles.DebugDraw,
        ["adminTools.lua"] = ResourceFiles.AdminTools,
        ["tools/debugWorld.lua"] = ResourceFiles.ToolsDebugWorld,
    };

    internal AdminToolsResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "AdminTools")
    {
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}
