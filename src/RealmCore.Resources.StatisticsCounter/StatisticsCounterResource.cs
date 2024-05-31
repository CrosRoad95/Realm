using SlipeServer.Server.Resources;
using SlipeServer.Server;

namespace RealmCore.Resources.StatisticsCounter;

internal class StatisticsCounterResource : Resource
{
    internal StatisticsCounterResource(MtaServer server) : base(server, server.RootElement, "StatisticsCounter") { }
}
