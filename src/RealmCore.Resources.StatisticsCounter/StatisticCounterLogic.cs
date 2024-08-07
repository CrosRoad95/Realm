﻿using Microsoft.Extensions.Logging;
using RealmCore.Resources.Base;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Events;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;

namespace RealmCore.Resources.StatisticsCounter;

internal class StatisticCounterLogic
{
    private readonly object _lock = new();
    private readonly LuaEventService _luaEventService;
    private readonly StatisticsCounterResource _resource;
    private readonly IStatisticsCounterService _statisticsCounterService;
    private readonly ILogger<StatisticCounterLogic> _logger;
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly HashSet<Player> _players = [];

    public StatisticCounterLogic(MtaServer server, LuaEventService luaEventService,
        IStatisticsCounterService statisticsCounterResource, ILogger<StatisticCounterLogic> logger, FromLuaValueMapper fromLuaValueMapper)
    {
        _luaEventService = luaEventService;
        _statisticsCounterService = statisticsCounterResource;
        _logger = logger;
        _fromLuaValueMapper = fromLuaValueMapper;
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<StatisticsCounterResource>();
        _statisticsCounterService.CounterStateChanged += HandleCounterStateChanged;
        luaEventService.AddEventHandler("internalCollectStatistics", HandleCollectStatistics);
        luaEventService.AddEventHandler("internalCollectFpsStatistics", HandleCollectFpsStatistics);
    }

    private void HandleCounterStateChanged(Player player, bool enabled)
    {
        lock (_lock)
        {
            if (enabled)
            {
                if (_players.Add(player))
                {
                    player.Disconnected += HandlePlayerQuit;
                    if (player.IsDestroyed)
                    {
                        player.Disconnected -= HandlePlayerQuit;
                        _players.Remove(player);
                        return;
                    }

                    _luaEventService.TriggerEventFor(player, "internalSetCounterEnabled", player, true);
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
    }

    private void HandlePlayerQuit(Player player, PlayerQuitEventArgs e)
    {
        lock (_lock)
        {
            if(_players.Remove(player))
                player.Disconnected -= HandlePlayerQuit;
        }
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }

    private void HandleCollectStatistics(LuaEvent luaEvent)
    {
        try
        {
            var table = luaEvent.Parameters.ElementAt(0)?.TableValue;
            if (table == null)
                return;
            var collectedStatistics = table.ToDictionary(x => x.Key.IntegerValue ?? throw new Exception(), x => float.Parse(x.Value.ToString()));
            _statisticsCounterService.RelayCollectedStatistics(luaEvent.Player, collectedStatistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect statistics for player serial: {serial}", luaEvent.Player.Client.Serial);
        }
    }

    private void HandleCollectFpsStatistics(LuaEvent luaEvent)
    {
        try
        {
            var (minFps, maxFps, avgFps) = luaEvent.Read<float, float, float>(_fromLuaValueMapper);

            if (minFps < 0 || maxFps < 0 || avgFps < 0 || minFps > 1000 || maxFps > 1000 || avgFps > 1000)
            {
                throw new ArgumentOutOfRangeException($"Failed to read fps, min: {minFps}, max: {maxFps}, avg: {avgFps}");
            }

            _statisticsCounterService.RelayFpsCollectedStatistics(luaEvent.Player, minFps, maxFps, avgFps);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect fps statistics for player serial: {serial}", luaEvent.Player.Client.Serial);

        }
    }
}