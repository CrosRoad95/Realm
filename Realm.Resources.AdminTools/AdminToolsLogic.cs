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
        adminToolsService.AdminToolsDisabled += AdminToolsService_AdminToolsDisabled;
        adminToolsService.AdminToolsEnabled += AdminToolsService_AdminToolsEnabled;
        mtaServer.LuaEventTriggered += MtaServer_LuaEventTriggered;
        mtaServer.ElementCreated += MtaServer_ElementCreated;
    }

    private void MtaServer_ElementCreated(Element element)
    {
        SendElementsDebugInfoToPlayers(element);
    }

    private LuaValue ElementToLuaValue(Element element)
    {
        var worldDebugData = element as IWorldDebugData;

        return new LuaValue(new Dictionary<LuaValue, LuaValue>
        {
            ["element"] = element,
            ["debugId"] = worldDebugData.DebugId.ToString(),
            ["name"] = element.ToString(),
            ["previewType"] = (int)worldDebugData.PreviewType,
            ["color"] = new LuaValue(new LuaValue[] { (int)worldDebugData.PreviewColor.R, (int)worldDebugData.PreviewColor.G, (int)worldDebugData.PreviewColor.B, (int)worldDebugData.PreviewColor.A }),
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
        player.Disconnected += Player_Disconnected;
        SendAllElementsDebugInfoToPlayer(player);
    }
    
    private void UnsubscribeToDrawWorld(Player player)
    {
        if (!_debugWorldSubscribers.Contains(player))
            return;
        _debugWorldSubscribers.Remove(player);
        player.Disconnected -= Player_Disconnected;
    }

    private void Player_Disconnected(Player sender, SlipeServer.Server.Elements.Events.PlayerQuitEventArgs e)
    {
        UnsubscribeToDrawWorld(sender);
    }

    private void MtaServer_LuaEventTriggered(LuaEvent luaEvent)
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

    private void AdminToolsService_AdminToolsEnabled(Player player)
    {
        _luaEventService.TriggerEventFor(player, "internalSetAdminToolsEnabled", player, true);
    }

    private void AdminToolsService_AdminToolsDisabled(Player player)
    {
        _luaEventService.TriggerEventFor(player, "internalSetAdminToolsEnabled", player, false);
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }
}