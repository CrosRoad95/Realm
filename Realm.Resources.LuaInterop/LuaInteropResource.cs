using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server;

namespace Realm.Resources.LuaInterop;

internal class LuaInteropResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["debugging.lua"] = ResourceFiles.Debugging,
        ["utility.lua"] = ResourceFiles.Utility,
    };

    internal LuaInteropResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "LuaInterop")
    {
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}
