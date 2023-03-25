using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;
using System.Drawing;

namespace Realm.Resources.ElementOutline;

internal class ElementOutlineLogic
{
    private readonly IElementOutlineService _elementOutlineService;
    private readonly LuaEventService _luaEventService;
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly ElementOutlineResource _resource;
    public ElementOutlineLogic(MtaServer server, LuaEventService luaEventService, FromLuaValueMapper fromLuaValueMapper, IElementOutlineService elementOutlineService)
    {
        _luaEventService = luaEventService;
        _fromLuaValueMapper = fromLuaValueMapper;
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<ElementOutlineResource>();
        _elementOutlineService = elementOutlineService;
        _elementOutlineService.OutlineChanged += HandleOutlineChanged;
        _elementOutlineService.OutlineRemoved += HandleOutlineRemoved;
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }

    private void HandleOutlineChanged(Player player, Element target, Color color)
    {
        _luaEventService.TriggerEventFor(player, "internalSetOutline", player, target, color);
    }

    private void HandleOutlineRemoved(Player player, Element target)
    {
        _luaEventService.TriggerEventFor(player, "internalRemoveOutline", player, target);
    }
}
