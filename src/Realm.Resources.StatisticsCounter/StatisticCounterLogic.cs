using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace Realm.Resources.StatisticsCounter;

internal class StatisticCounterLogic
{
    private readonly LuaEventService _luaEventService;
    private readonly StatisticsCounterResource _resource;
    private readonly StatisticsCounterService _statisticsCounterService;
    private readonly HashSet<Player> _players = new();

    public StatisticCounterLogic(MtaServer server, LuaEventService luaEventService,
        StatisticsCounterService statisticsCounterResource)
    {
        _luaEventService = luaEventService;
        _statisticsCounterService = statisticsCounterResource;
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<StatisticsCounterResource>();
        _statisticsCounterService.CounterStateChanged += HandleCounterStateChanged;
        luaEventService.AddEventHandler("internalCollectStatistics", HandleCollectStatistics);
    }

    private void HandleCounterStateChanged(Player player, bool enabled)
    {
        if (enabled)
        {
            if (_players.Add(player))
            {
                _luaEventService.TriggerEventFor(player, "internalSetCounterEnabled", player, true);
                player.Disconnected += HandlePlayerQuit;
            }
        }
        else
        {
            if (_players.Remove(player))
            {
                _luaEventService.TriggerEventFor(player, "internalSetCounterEnabled", player, false);
                player.Disconnected -= HandlePlayerQuit;
            }
        }
    }

    private void HandlePlayerQuit(Player player, PlayerQuitEventArgs e)
    {
        _players.Remove(player);
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }

    private void HandleCollectStatistics(LuaEvent luaEvent)
    {
        // TODO: Put into try catch, catch exceptions, log them
        var collectedStatistics = luaEvent.Parameters.First().TableValue
            .ToDictionary(x => x.Key.ToString(), x => float.Parse(x.Value.ToString()));

        _statisticsCounterService.RelayCollectedStatistics(luaEvent.Player, collectedStatistics);
    }
}