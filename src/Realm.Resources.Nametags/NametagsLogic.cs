using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;
using System.Collections.Concurrent;

namespace Realm.Resources.Nametags;

internal class NametagsLogic
{
    private readonly LuaEventService _luaEventService;
    private readonly NametagsService _nametagsService;
    private readonly ILogger<NametagsLogic> _logger;
    private readonly RootElement _rootElement;
    private readonly NametagsResource _resource;
    private readonly object _lock = new();
    private LuaValue _nametagsCache = new(new Dictionary<LuaValue, LuaValue>());
    
    //private readonly Dictionary<Ped, Nametag> _nametags = new();

    public NametagsLogic(MtaServer mtaServer, LuaEventService luaEventService, NametagsService nametagsService, ILogger<NametagsLogic> logger,
        RootElement rootElement)
    {
        _luaEventService = luaEventService;
        _nametagsService = nametagsService;
        _logger = logger;
        _rootElement = rootElement;
        _resource = mtaServer.GetAdditionalResource<NametagsResource>();

        mtaServer.PlayerJoined += HandlePlayerJoin;

        nametagsService.HandleSetNametag = HandleSetNametag;
        nametagsService.HandleRemoveNametag = HandleRemoveNametag;
        nametagsService.HandleSetNametagRenderingEnabled = HandleSetNametagRenderingEnabled;
    }

    private void HandleSetNametagRenderingEnabled(Player player, bool enabled)
    {
        player.TriggerLuaEvent("internalSetNametagRenderingEnabled", player, enabled);
    }
    
    private void HandleRemoveNametag(Ped ped)
    {
        lock (_lock)
            _nametagsCache.TableValue.Remove(ped);
        _luaEventService.TriggerEvent("internalRemovePedNametag", _rootElement, ped);
    }

    public void ResendAllNametagsToAllPlayer(Player player)
    {
        lock(_lock)
        {
            if(_nametagsCache.TableValue.Count > 0)
                player.TriggerLuaEvent("internalResendAllNametags", player, _nametagsCache);
        }
    }
    
    public void ResendPedNametagToAllPlayers(Ped ped)
    {
        lock(_lock)
            _luaEventService.TriggerEvent("internalSendPedNametags", _rootElement, ped, _nametagsCache.TableValue[ped]);
    }

    private void HandleSetNametag(Ped ped, string text)
    {
        var nametag = new Nametag
        {
            Text = text
        };

        lock(_lock)
            _nametagsCache.TableValue[ped] = nametag.LuaValue;
        ResendPedNametagToAllPlayers(ped);
    }

    private async void HandlePlayerJoin(Player player)
    {
        try
        {
            await _resource.StartForAsync(player);
            player.Disconnected += HandleDisconnected;
            ResendAllNametagsToAllPlayer(player);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to start nametags resource for player {playerName}", player.Name);
        }
    }

    private void HandleDisconnected(Player player, SlipeServer.Server.Elements.Events.PlayerQuitEventArgs e)
    {
        lock(_lock)
            _nametagsCache.TableValue.Remove(player);
    }
}
