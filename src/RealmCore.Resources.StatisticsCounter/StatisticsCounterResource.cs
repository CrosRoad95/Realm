using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server;

namespace RealmCore.Resources.StatisticsCounter;

internal class StatisticsCounterResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["counter.lua"] = ResourceFiles.Counter,
        ["fpsCounter.lua"] = ResourceFiles.FpsCounter,
    };

    internal StatisticsCounterResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "StatisticsCounter")
    {
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}
