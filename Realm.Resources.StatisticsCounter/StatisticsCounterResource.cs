using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server;

namespace Realm.Resources.StatisticsCounter;

internal class StatisticsCounterResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["counter.lua"] = ResourceFiles.Counter,
    };

    internal StatisticsCounterResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "StatisticsCounter")
    {
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}
