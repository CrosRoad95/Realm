using Microsoft.Extensions.Logging;
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
    private readonly ILogger<StatisticCounterLogic> _logger;
    private readonly HashSet<Player> _players = new();

    public StatisticCounterLogic(MtaServer server, LuaEventService luaEventService,
        StatisticsCounterService statisticsCounterResource, ILogger<StatisticCounterLogic> logger)
    {
        _luaEventService = luaEventService;
        _statisticsCounterService = statisticsCounterResource;
        _logger = logger;
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<StatisticsCounterResource>();
        _statisticsCounterService.CounterStateChanged += HandleCounterStateChanged;
        luaEventService.AddEventHandler("internalCollectStatistics", HandleCollectStatistics);
        luaEventService.AddEventHandler("internalCollectFpsStatistics", HandleCollectFpsStatistics);
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
        try
        {
            var collectedStatistics = luaEvent.Parameters[0].TableValue
                .ToDictionary(x => x.Key.IntegerValue ?? throw new Exception(), x => float.Parse(x.Value.ToString()));
            _statisticsCounterService.RelayCollectedStatistics(luaEvent.Player, collectedStatistics);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to collect statistics for player serial: {serial}", luaEvent.Player.Client.Serial);
        }
    }

    private void HandleCollectFpsStatistics(LuaEvent luaEvent)
    {
        try
        {
            var minFps = luaEvent.Parameters[0].FloatValue ?? throw new Exception("Failed to read minimal fps");
            var maxFps = luaEvent.Parameters[1].FloatValue ?? throw new Exception("Failed to read maximum fps");
            var avgFps = luaEvent.Parameters[2].FloatValue ?? throw new Exception("Failed to read average fps");
            if(minFps < 0 || maxFps < 0 || avgFps < 0 || minFps > 1000 || maxFps > 1000 || avgFps > 1000)
            {
                throw new ArgumentOutOfRangeException($"Failed to read fps, min: {minFps}, max: {maxFps}, avg: {avgFps}");
            }

            _statisticsCounterService.RelayFpsCollectedStatistics(luaEvent.Player, minFps, maxFps, avgFps);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to collect fps statistics for player serial: {serial}", luaEvent.Player.Client.Serial);

        }
    }
}