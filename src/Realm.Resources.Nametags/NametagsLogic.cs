using Microsoft.Extensions.Logging;
using Realm.Resources.Base;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.Elements;

namespace Realm.Resources.Nametags;

internal class NametagsLogic
{
    private readonly ILogger<NametagsLogic> _logger;
    private readonly ILuaEventHub<INametagsEventHub> _luaEventHub;
    private readonly NametagsResource _resource;
    private readonly object _lock = new();
    private LuaValue _nametagsCache = new(new Dictionary<LuaValue, LuaValue>());
    
    public NametagsLogic(MtaServer mtaServer, INametagsService nametagsService, ILogger<NametagsLogic> logger, ILuaEventHub<INametagsEventHub> luaEventHub)
    {
        _logger = logger;
        _luaEventHub = luaEventHub;
        _resource = mtaServer.GetAdditionalResource<NametagsResource>();

        mtaServer.PlayerJoined += HandlePlayerJoin;

        nametagsService.HandleSetNametag = HandleSetNametag;
        nametagsService.HandleRemoveNametag = HandleRemoveNametag;
        nametagsService.HandleSetNametagRenderingEnabled = HandleSetNametagRenderingEnabled;
        nametagsService.HandleSetLocalPlayerRenderingEnabled = HandleSetLocalPlayerRenderingEnabled;
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
            _nametagsCache.TableValue!.Remove(ped);
        _luaEventHub.Broadcast(x => x.RemoveNametag(), ped);
    }

    public void ResendAllNametagsToAllPlayer(Player player)
    {
        lock(_lock)
        {
            if (_nametagsCache.TableValue!.Count > 0)
                _luaEventHub.Invoke(player, x => x.AddNametags(_nametagsCache));
        }
    }
    
    public void ResendPedNametagToAllPlayers(Ped ped)
    {
        lock(_lock)
        {
            var value = _nametagsCache.TableValue![ped];
            _luaEventHub.Broadcast(x => x.SetPedNametag(value), ped);
        }
    }

    private void HandleSetNametag(Ped ped, string text)
    {
        var nametag = new Nametag
        {
            Text = text
        };

        lock(_lock)
            _nametagsCache.TableValue![ped] = nametag.LuaValue;
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
            _logger.LogError(ex, "Failed to start nametags resource for player: {playerName}, serial: {playerSerial}", player.Name, player.Client.Serial);
        }
    }

    private void HandleDisconnected(Player player, SlipeServer.Server.Elements.Events.PlayerQuitEventArgs e)
    {
        lock(_lock)
            _nametagsCache.TableValue!.Remove(player);
    }
}
