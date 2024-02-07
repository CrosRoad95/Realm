using RealmCore.Resources.Base;
using RealmCore.Resources.Base.Extensions;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Events;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;

namespace RealmCore.Resources.Admin;

internal class AdminLogic
{
    private readonly AdminResource _resource;
    private readonly LuaEventService _luaEventService;
    private readonly IAdminService _adminService;
    private readonly IElementCollection _elementCollection;
    private readonly ILuaEventHub<IAdminEventHub> _luaEventHub;
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly LuaValueMapper _luaValueMapper;
    private readonly object _debugWorldSubscribersLock = new();
    private readonly List<Player> _debugWorldSubscribers = [];
    private readonly object _enabledForPlayersLock = new();
    private readonly List<Player> _enabledForPlayers = [];

    public AdminLogic(MtaServer mtaServer, LuaEventService luaEventService, IAdminService adminService, IElementCollection elementCollection, ILuaEventHub<IAdminEventHub> luaEventHub, FromLuaValueMapper fromLuaValueMapper, LuaValueMapper luaValueMapper)
    {
        mtaServer.PlayerJoined += HandlePlayerJoin;
        _resource = mtaServer.GetAdditionalResource<AdminResource>();
        _luaEventService = luaEventService;
        _adminService = adminService;
        _elementCollection = elementCollection;
        _luaEventHub = luaEventHub;
        _fromLuaValueMapper = fromLuaValueMapper;
        _luaValueMapper = luaValueMapper;
        _adminService.MessageHandler = HandleMessage;

        luaEventService.AddEventHandler("internalSetToolState", HandleSetToolState);

        _luaValueMapper.DefineStructMapper<ElementDebugInfo>(ElementDebugInfoToLuaValue);
    }

    private void HandleMessage(IMessage message)
    {
        // TODO: add loggers
        switch(message)
        {
            case AdminModeChangedMessage adminModeChangedMessage:
                lock (_enabledForPlayersLock)
                {
                    if (IsAdminEnabledForPlayer(adminModeChangedMessage.Player) == adminModeChangedMessage.State)
                        return;

                    _enabledForPlayers.Add(adminModeChangedMessage.Player);
                    _luaEventHub.Invoke(adminModeChangedMessage.Player, x => x.SetAdminEnabled(adminModeChangedMessage.State));
                }
                break;
            case SetAdminToolsMessage setAdminToolsMessage:
                lock (_enabledForPlayersLock)
                {
                    if (!IsAdminEnabledForPlayer(setAdminToolsMessage.Player))
                        return;
                    var tools = setAdminToolsMessage.AdminTools.Select(x => (int)x);
                    _luaEventHub.Invoke(setAdminToolsMessage.Player, x => x.SetTools(tools));
                }
                break;
            case BroadcastElementDebugInfoMessage broadcastElementDebugInfoMessage:
                lock (_enabledForPlayersLock)
                {
                    if (_enabledForPlayers.Count == 0)
                        return;

                    var luaValue = new LuaValue[] { _luaValueMapper.Map(broadcastElementDebugInfoMessage.ElementDebugInfo) };
                    _luaEventHub.Invoke(_enabledForPlayers, x => x.AddOrUpdateElement(luaValue));
                }
                break;
            case BroadcastElementsDebugInfoMessage broadcastElementsDebugInfoMessage:
                lock (_enabledForPlayersLock)
                {
                    if (_enabledForPlayers.Count == 0)
                        return;

                    var luaValues = broadcastElementsDebugInfoMessage.ElementsDebugInfo.Select(x => _luaValueMapper.Map(x));
                    _luaEventHub.Invoke(_enabledForPlayers, x => x.AddOrUpdateElement(luaValues));
                }
                break;
            case BroadcastElementDebugInfoMessageForPlayer broadcastElementDebugInfoMessageForPlayer:
                lock (_enabledForPlayersLock)
                {
                    var player = broadcastElementDebugInfoMessageForPlayer.player;
                    if (!_enabledForPlayers.Contains(player))
                        return;

                    var luaValue = new LuaValue[] { _luaValueMapper.Map(broadcastElementDebugInfoMessageForPlayer.ElementDebugInfo) };
                    _luaEventHub.Invoke(_enabledForPlayers, x => x.AddOrUpdateElement(luaValue));
                }
                break;
            case BroadcastElementsDebugInfoMessageForPlayer broadcastElementsDebugInfoMessageForPlayer:
                lock (_enabledForPlayersLock)
                {
                    var player = broadcastElementsDebugInfoMessageForPlayer.player;
                    if (!_enabledForPlayers.Contains(player))
                        return;

                    var luaValues = broadcastElementsDebugInfoMessageForPlayer.ElementsDebugInfo.Select(x => _luaValueMapper.Map(x));
                    _luaEventHub.Invoke(player, x => x.AddOrUpdateElement(luaValues));
                }
                break;
            case ClearElementsForPlayerMessage clearElementsForPlayerMessage:
                _luaEventHub.Invoke(clearElementsForPlayerMessage.player, x => x.ClearElements());
                break;
            case BroadcastSpawnMarkersForPlayerMessage broadcastSpawnMarkersForPlayerMessage:
                var markers = broadcastSpawnMarkersForPlayerMessage.SpawnMarkers;
                _luaEventHub.Invoke(broadcastSpawnMarkersForPlayerMessage.Player, x=> x.SetSpawnMarkers(markers));
                break;
            case ClearSpawnMarkersForPlayerMessage clearSpawnMarkersForPlayerMessage:
                _luaEventHub.Invoke(clearSpawnMarkersForPlayerMessage.Player, x => x.ClearSpawnMarkers());
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private bool IsAdminEnabledForPlayer(Player player) => _enabledForPlayers.Contains(player);

    private LuaValue ElementDebugInfoToLuaValue(ElementDebugInfo elementDebugInfo)
    {
        var data = new Dictionary<LuaValue, LuaValue>
        {
            ["debugId"] = elementDebugInfo.debugId,
            ["name"] = elementDebugInfo.name,
            ["previewType"] = (int)elementDebugInfo.previewType,
            ["color"] = elementDebugInfo.previewColor.ToLuaColor(),
            ["position"] = new LuaValue(new LuaValue[] { elementDebugInfo.position.X, elementDebugInfo.position.Y, elementDebugInfo.position.Z }),
        };
        if (elementDebugInfo.element != null)
            data["element"] = elementDebugInfo.element;

        return new LuaValue(data);
    }

    private void HandleSetToolState(LuaEvent luaEvent)
    {
        var (tool, state) = luaEvent.Read<AdminTool, bool>(_fromLuaValueMapper);
        _adminService.RelayToolStateChanged(luaEvent.Player, tool, state);
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }
}