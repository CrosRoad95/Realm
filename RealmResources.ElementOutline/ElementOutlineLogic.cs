using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;
using System.Drawing;

namespace RealmResources.ElementOutline;

internal class ElementOutlineLogic
{
    private readonly ElementOutlineService _elementOutlineService;
    private readonly LuaEventService _luaEventService;
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly ElementOutlineResource _resource;
    public ElementOutlineLogic(MtaServer server, LuaEventService luaEventService, FromLuaValueMapper fromLuaValueMapper, ElementOutlineService elementOutlineService)
    {
        _luaEventService = luaEventService;
        _fromLuaValueMapper = fromLuaValueMapper;
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<ElementOutlineResource>();
        _elementOutlineService = elementOutlineService;
        _elementOutlineService.OutlineChanged += ElementOutlineService_OutlineChanged;
        _elementOutlineService.OutlineRemoved += ElementOutlineService_OutlineRemoved;
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }

    private void ElementOutlineService_OutlineChanged(Player player, Element target, Color color)
    {
        _luaEventService.TriggerEventFor(player, "internalSetOutline", player, target, color);
    }

    private void ElementOutlineService_OutlineRemoved(Player player, Element target)
    {
        _luaEventService.TriggerEventFor(player, "internalRemoveOutline", player, target);
    }
}
