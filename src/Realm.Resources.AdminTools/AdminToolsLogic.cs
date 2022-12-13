using Realm.Resources.AdminTools.Interfaces;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace Realm.Resources.AdminTools;

internal class AdminToolsLogic
{
    private readonly AdminToolsResource _resource;
    private readonly LuaEventService _luaEventService;
    private readonly IElementCollection _elementCollection;
    private readonly List<Player> _debugWorldSubscribers = new();

    public AdminToolsLogic(MtaServer mtaServer, LuaEventService luaEventService, AdminToolsService adminToolsService, IElementCollection elementCollection)
    {
        mtaServer.PlayerJoined += HandlePlayerJoin;
        _resource = mtaServer.GetAdditionalResource<AdminToolsResource>();
        _luaEventService = luaEventService;
        _elementCollection = elementCollection;
        adminToolsService.AdminToolsDisabled += HandleAdminToolsDisabled;
        adminToolsService.AdminToolsEnabled += HandleAdminToolsEnabled;
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
        _luaEventService.TriggerEventFor(player, "internalAddOrUpdateDebugElements", player, debugData);
    }

    private void SendElementsDebugInfoToPlayers(params Element[] elements)
    {
        var debugData = elements.Select(ElementToLuaValue);
        foreach (var player in _debugWorldSubscribers)
            _luaEventService.TriggerEventFor(player, "internalAddOrUpdateDebugElements", player, debugData);
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
        switch(luaEvent.Name)
        {
            case "internalDrawWorldSubscribe":
                SubscribeToDrawWorld(luaEvent.Player);
                break;
            case "internalDrawWorldUnsubscribe":
                UnsubscribeToDrawWorld(luaEvent.Player);
                break;
        }
    }

    private void HandleAdminToolsEnabled(Player player)
    {
        _luaEventService.TriggerEventFor(player, "internalSetAdminToolsEnabled", player, true);
    }

    private void HandleAdminToolsDisabled(Player player)
    {
        _luaEventService.TriggerEventFor(player, "internalSetAdminToolsEnabled", player, false);
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }
}