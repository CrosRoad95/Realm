using RealmCore.Resources.Admin.Data;
using RealmCore.Resources.Admin.Interfaces;
using RealmCore.Resources.Base;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace RealmCore.Resources.Admin;

internal class AdminLogic
{
    private readonly AdminResource _resource;
    private readonly LuaEventService _luaEventService;
    private readonly IElementCollection _elementCollection;
    private readonly ILuaEventHub<IAdminEventHub> _luaEventHub;
    private readonly List<Player> _debugWorldSubscribers = new();

    public AdminLogic(MtaServer mtaServer, LuaEventService luaEventService, IAdminService adminService, IElementCollection elementCollection, ILuaEventHub<IAdminEventHub> luaEventHub)
    {
        mtaServer.PlayerJoined += HandlePlayerJoin;
        _resource = mtaServer.GetAdditionalResource<AdminResource>();
        _luaEventService = luaEventService;
        _elementCollection = elementCollection;
        _luaEventHub = luaEventHub;
        adminService.AdminDisabled += HandleAdminDisabled;
        adminService.AdminEnabled += HandleAdminEnabled;
        mtaServer.LuaEventTriggered += HandleLuaEventTriggered;
        mtaServer.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(Element element)
    {
        SendElementsDebugInfoToPlayers(element);
    }

    private LuaValue ElementToLuaValue(Element element)
    {
        var worldDebugData = element as IWorldDebugData;
        var debugData = worldDebugData.DebugData;
        return new LuaValue(new Dictionary<LuaValue, LuaValue>
        {
            ["element"] = element,
            ["debugId"] = debugData.DebugId.ToString(),
            ["name"] = element.ToString(),
            ["previewType"] = (int)debugData.PreviewType,
            ["color"] = new LuaValue(new LuaValue[] { (int)debugData.PreviewColor.R, (int)debugData.PreviewColor.G, (int)debugData.PreviewColor.B, (int)debugData.PreviewColor.A }),
            ["type"] = element.GetType().Name,
            ["position"] = new LuaValue(new LuaValue[] { worldDebugData.Position.X, worldDebugData.Position.Y, worldDebugData.Position.Z }),
        });
    }

    private void SendAllElementsDebugInfoToPlayer(Player player)
    {
        var debugData = _elementCollection.GetAll()
            .Where(x => x is IWorldDebugData)
            .Select(ElementToLuaValue);
        _luaEventHub.Invoke(player, x => x.InternalAddOrUpdateDebugElements(debugData));
    }

    private void SendElementsDebugInfoToPlayers(params Element[] elements)
    {
        var debugData = elements.Select(ElementToLuaValue);
        foreach (var player in _debugWorldSubscribers)
            _luaEventHub.Invoke(player, x => x.InternalAddOrUpdateDebugElements(debugData));
    }

    private void SubscribeToDrawWorld(Player player)
    {
        if (_debugWorldSubscribers.Contains(player))
            return;
        _debugWorldSubscribers.Add(player);
        player.Disconnected += HandlePlayerDisconnected;
        SendAllElementsDebugInfoToPlayer(player);
    }

    private void UnsubscribeToDrawWorld(Player player)
    {
        if (!_debugWorldSubscribers.Contains(player))
            return;
        _debugWorldSubscribers.Remove(player);
        player.Disconnected -= HandlePlayerDisconnected;
    }

    private void HandlePlayerDisconnected(Player sender, SlipeServer.Server.Elements.Events.PlayerQuitEventArgs e)
    {
        UnsubscribeToDrawWorld(sender);
    }

    private void HandleLuaEventTriggered(LuaEvent luaEvent)
    {
        switch (luaEvent.Name)
        {
            case "internalDrawWorldSubscribe":
                SubscribeToDrawWorld(luaEvent.Player);
                break;
            case "internalDrawWorldUnsubscribe":
                UnsubscribeToDrawWorld(luaEvent.Player);
                break;
        }
    }

    private void HandleAdminEnabled(Player player)
    {
        _luaEventHub.Invoke(player, x => x.InternalSetAdminEnabled(true));
    }

    private void HandleAdminDisabled(Player player)
    {
        _luaEventHub.Invoke(player, x => x.InternalSetAdminEnabled(false));
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }
}