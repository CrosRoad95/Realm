using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;

namespace Realm.Resources.StatisticsCounter;

internal class StatisticCounterLogic
{
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly StatisticsCounterResource _resource;
    private readonly StatisticsCounterService _statisticsCounterResource;

    public StatisticCounterLogic(MtaServer server, LuaEventService luaEventService, FromLuaValueMapper fromLuaValueMapper,
        StatisticsCounterService statisticsCounterResource)
    {
        _fromLuaValueMapper = fromLuaValueMapper;
        _statisticsCounterResource = statisticsCounterResource;
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<StatisticsCounterResource>();
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }
}