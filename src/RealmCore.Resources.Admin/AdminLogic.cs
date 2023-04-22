using RealmCore.Resources.Admin.Data;
using RealmCore.Resources.Admin.Interfaces;
using RealmCore.Resources.Admin.Messages;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;
using System.Linq;

namespace RealmCore.Resources.Admin;

internal class AdminLogic
{
    private readonly AdminResource _resource;
    private readonly LuaEventService _luaEventService;
    private readonly IElementCollection _elementCollection;
    private readonly ILuaEventHub<IAdminEventHub> _luaEventHub;
    private readonly List<Player> _debugWorldSubscribers = new();

    private readonly HashSet<Player> _enabledForPlayers = new();
    private readonly object _lock = new();

    public AdminLogic(MtaServer mtaServer, LuaEventService luaEventService, IAdminService adminService, IElementCollection elementCollection, ILuaEventHub<IAdminEventHub> luaEventHub)
    {
        mtaServer.PlayerJoined += HandlePlayerJoin;
        _resource = mtaServer.GetAdditionalResource<AdminResource>();
        _luaEventService = luaEventService;
        _elementCollection = elementCollection;
        _luaEventHub = luaEventHub;
        adminService.MessageHandler = HandleMessage;
        mtaServer.LuaEventTriggered += HandleLuaEventTriggered;
        mtaServer.ElementCreated += HandleElementCreated;
    }

    private void HandleMessage(IMessage message)
    {
        switch(message)
        {
            case AdminModeChangedMessage adminModeChangedMessage:
                lock (_lock)
                {
                    if (IsAdminEnabledForPlayer(adminModeChangedMessage.Player) == adminModeChangedMessage.State)
                        return;

                    _enabledForPlayers.Add(adminModeChangedMessage.Player);
                    _luaEventHub.Invoke(adminModeChangedMessage.Player, x => x.SetAdminEnabled(adminModeChangedMessage.State));
                }
                break;
            case SetAdminToolsMessage setAdminToolsMessage:
                lock (_lock)
                {
                    if (!IsAdminEnabledForPlayer(setAdminToolsMessage.Player))
                        return;
                    var tools = setAdminToolsMessage.AdminTools.Select(x => (int)x);
                    _luaEventHub.Invoke(setAdminToolsMessage.Player, x => x.SetTools(tools));
                }
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private bool IsAdminEnabledForPlayer(Player player) => _enabledForPlayers.Contains(player);

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
        _luaEventHub.Invoke(player, x => x.AddOrUpdateDebugElements(debugData));
    }

    private void SendElementsDebugInfoToPlayers(params Element[] elements)
    {
        var debugData = elements.Select(ElementToLuaValue);
        foreach (var player in _debugWorldSubscribers)
            _luaEventHub.Invoke(player, x => x.AddOrUpdateDebugElements(debugData));
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
        _luaEventHub.Invoke(player, x => x.SetAdminEnabled(true));
    }

    private void HandleAdminDisabled(Player player)
    {
        _luaEventHub.Invoke(player, x => x.SetAdminEnabled(false));
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }
}