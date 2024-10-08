﻿using Microsoft.Extensions.Logging;
using RealmCore.Resources.Base.Extensions;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Resources.Base;

namespace RealmCore.Resources.Nametags;

internal class NametagsLogic
{
    private readonly INametagsService _nametagsService;
    private readonly ILogger<NametagsLogic> _logger;
    private readonly ILuaEventHub<INametagsEventHub> _luaEventHub;
    private readonly NametagsResource _resource;
    private readonly object _lock = new();
    private readonly Dictionary<Ped, Nametag> _nametagsCache = [];

    public NametagsLogic(MtaServer server, INametagsService nametagsService, ILogger<NametagsLogic> logger, ILuaEventHub<INametagsEventHub> luaEventHub)
    {
        _nametagsService = nametagsService;
        _logger = logger;
        _luaEventHub = luaEventHub;
        _resource = server.GetAdditionalResource<NametagsResource>();

        server.PlayerJoined += HandlePlayerJoin;

        nametagsService.RelaySetNametag = HandleSetNametag;
        nametagsService.RelayRemoveNametag = HandleRemoveNametag;
        nametagsService.RelaySetNametagRenderingEnabled = HandleSetNametagRenderingEnabled;
        nametagsService.RelaySetLocalPlayerRenderingEnabled = HandleSetLocalPlayerRenderingEnabled;
        nametagsService.RelayResendAllNametagsToPlayer = ResendAllNametagsToPlayer;
    }

    private void HandleSetNametagRenderingEnabled(Player player, bool enabled)
    {
        _luaEventHub.Invoke(player, x => x.SetRenderingEnabled(enabled));
    }

    private void HandleSetLocalPlayerRenderingEnabled(Player player, bool enabled)
    {
        _luaEventHub.Invoke(player, x => x.SetLocalPlayerRenderingEnabled(enabled));
    }

    private void HandleRemoveNametag(Ped ped)
    {
        lock (_lock)
        {
            if (!_nametagsCache.Remove(ped))
                return;
        }
        _luaEventHub.Broadcast(x => x.RemoveNametag(), ped);
    }

    public void ResendAllNametagsToPlayer(Player player)
    {
        lock (_lock)
        {
            if (_nametagsCache.Count == 0)
                return;

            var luaValue = new LuaValue(_nametagsCache.ToDictionary(x => new LuaValue(x.Key.Id), y => y.Value.LuaValue));
            _luaEventHub.Invoke(player, x => x.AddNametags(luaValue));
        }
    }

    public void ResendPedNametagToAllPlayers(Ped ped)
    {
        lock (_lock)
        {
            if(_nametagsCache.TryGetValue(ped, out var nametag))
            {
                var luaValue = nametag.LuaValue;
                _luaEventHub.Broadcast(x => x.SetPedNametag(luaValue), ped);
            }
        }
    }

    private void HandleSetNametag(Ped ped, string text)
    {
        var nametag = new Nametag
        {
            Text = text
        };

        lock (_lock)
        {
            _nametagsCache[ped] = nametag;
        }
        ResendPedNametagToAllPlayers(ped);
    }

    private async void HandlePlayerJoin(Player player)
    {
        try
        {
            await _resource.StartForAsync(player);
            player.Disconnected += HandleDisconnected;
            if (player.NametagText != null)
                _nametagsService.SetNametag(player, player.NametagText);
            ResendAllNametagsToPlayer(player);

        }
        catch (Exception ex)
        {
            _logger.ResourceFailedToStart<NametagsResource>(ex);
        }
    }

    private void HandleDisconnected(Player player, SlipeServer.Server.Elements.Events.PlayerQuitEventArgs e)
    {
        player.Disconnected -= HandleDisconnected;
        lock (_lock)
        {
            _nametagsCache.Remove(player);
        }
    }
}
