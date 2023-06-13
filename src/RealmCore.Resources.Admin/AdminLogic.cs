using RealmCore.Resources.Admin.Data;
using RealmCore.Resources.Admin.Enums;
using RealmCore.Resources.Admin.Messages;
using RealmCore.Resources.Base;
using RealmCore.Resources.Base.Extensions;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
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
    private readonly List<Player> _debugWorldSubscribers = new();
    private readonly object _enabledForPlayersLock = new();
    private readonly List<Player> _enabledForPlayers = new();

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

        _luaValueMapper.DefineStructMapper<EntityDebugInfo>(EntityDebugInfoToLuaValue);
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
            case BroadcastEntityDebugInfoMessage broadcastEntityDebugInfoMessage:
                lock (_enabledForPlayersLock)
                {
                    if (!_enabledForPlayers.Any())
                        return;

                    var luaValue = new LuaValue[] { _luaValueMapper.Map(broadcastEntityDebugInfoMessage.EntityDebugInfo) };
                    _luaEventHub.Invoke(_enabledForPlayers, x => x.AddOrUpdateEntity(luaValue));
                }
                break;
            case BroadcastEntitiesDebugInfoMessage broadcastEntitiesDebugInfoMessage:
                lock (_enabledForPlayersLock)
                {
                    if (!_enabledForPlayers.Any())
                        return;

                    var luaValues = broadcastEntitiesDebugInfoMessage.EntitiesDebugInfo.Select(x => _luaValueMapper.Map(x));
                    _luaEventHub.Invoke(_enabledForPlayers, x => x.AddOrUpdateEntity(luaValues));
                }
                break;
            case BroadcastEntityDebugInfoMessageForPlayer broadcastEntityDebugInfoMessageForPlayer:
                lock (_enabledForPlayersLock)
                {
                    var player = broadcastEntityDebugInfoMessageForPlayer.player;
                    if (!_enabledForPlayers.Contains(player))
                        return;

                    var luaValue = new LuaValue[] { _luaValueMapper.Map(broadcastEntityDebugInfoMessageForPlayer.EntityDebugInfo) };
                    _luaEventHub.Invoke(_enabledForPlayers, x => x.AddOrUpdateEntity(luaValue));
                }
                break;
            case BroadcastEntitiesDebugInfoMessageForPlayer broadcastEntitiesDebugInfoMessageForPlayer:
                lock (_enabledForPlayersLock)
                {
                    var player = broadcastEntitiesDebugInfoMessageForPlayer.player;
                    if (!_enabledForPlayers.Contains(player))
                        return;

                    var luaValues = broadcastEntitiesDebugInfoMessageForPlayer.EntitiesDebugInfo.Select(x => _luaValueMapper.Map(x));
                    _luaEventHub.Invoke(player, x => x.AddOrUpdateEntity(luaValues));
                }
                break;
            case ClearEntitiesForPlayerMessage clearEntitiesForPlayerMessage:
                _luaEventHub.Invoke(clearEntitiesForPlayerMessage.player, x => x.ClearEntities());
                break;
            case BroadcastSpawnMarkersForPlayerMessage broadcastSpawnMarkersForPlayerMessage:
                var markers = broadcastSpawnMarkersForPlayerMessage.SpawnMarkers;
                _luaEventHub.Invoke(broadcastSpawnMarkersForPlayerMessage.Player, x=> x.SetSpawnMarkers(markers));
                break;
            case ClearSpawnMarkersForPlayerMessage clearSpawnMarkersForPlayerMessage:
                _luaEventHub.Invoke(clearSpawnMarkersForPlayerMessage.Player, x => x.ClearSpawnMarkers());
                break;
            case UpdateEntitiesComponentsMessage updateEntitiesComponentsMessage:
                var components = updateEntitiesComponentsMessage.entitiesComponents;
                _luaEventHub.Invoke(updateEntitiesComponentsMessage.Player, x => x.UpdateEntitiesComponents(components));
                break;
            case ClearEntitiesComponentsMessage clearEntitiesComponentsMessage:
                _luaEventHub.Invoke(clearEntitiesComponentsMessage.Player, x => x.ClearEntitiesComponents());
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private bool IsAdminEnabledForPlayer(Player player) => _enabledForPlayers.Contains(player);

    private LuaValue EntityDebugInfoToLuaValue(EntityDebugInfo entityDebugInfo)
    {
        var data = new Dictionary<LuaValue, LuaValue>
        {
            ["debugId"] = entityDebugInfo.debugId,
            ["name"] = entityDebugInfo.name,
            ["previewType"] = (int)entityDebugInfo.previewType,
            ["color"] = entityDebugInfo.previewColor.ToLuaColor(),
            ["position"] = new LuaValue(new LuaValue[] { entityDebugInfo.position.X, entityDebugInfo.position.Y, entityDebugInfo.position.Z }),
        };
        if (entityDebugInfo.element != null)
            data["element"] = entityDebugInfo.element;

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