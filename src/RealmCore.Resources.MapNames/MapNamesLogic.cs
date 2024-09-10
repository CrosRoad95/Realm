using RealmCore.Resources.Base.Extensions;

namespace RealmCore.Resources.MapNames;

internal sealed class MapNamesLogic
{
    private readonly LuaEventService _luaEventService;
    private readonly IMapNamesService _mapNamesService;
    private readonly ILogger<MapNamesLogic> _logger;
    private readonly ILuaEventHub<IMapNamesEventHub> _luaEventHub;
    private readonly MapNamesResource _resource;

    private ConcurrentDictionary<MapNameId, MapName> _mapNames = [];

    public MapNamesLogic(MtaServer server, LuaEventService luaEventService, IMapNamesService mapNamesService, ILogger<MapNamesLogic> logger, ILuaEventHub<IMapNamesEventHub> luaEventHub)
    {
        _luaEventService = luaEventService;
        _mapNamesService = mapNamesService;
        _logger = logger;
        _luaEventHub = luaEventHub;
        _resource = server.GetAdditionalResource<MapNamesResource>();

        server.PlayerJoined += HandlePlayerJoin;

        _mapNamesService.Added += HandleAdded;
        _mapNamesService.AddedFor += HandleAddedFor;
        _mapNamesService.Removed += HandleRemoved;
        _mapNamesService.RemovedFor += HandleRemovedFor;
        _mapNamesService.CategoryVisibilityChanged += HandleCategoryVisibilityChanged;
        _mapNamesService.NameChanged += HandleNameChanged;
        _mapNamesService.NameChangedFor += HandleNameChangedFor;
    }

    private void HandleCategoryVisibilityChanged(Player player, int[] categories)
    {
        _luaEventHub.Invoke(player, x => x.SetVisibleCategories(categories));
    }

    private void HandleRemovedFor(MapNameId id, Player[] players)
    {
        _luaEventHub.Invoke(players, x => x.Remove(id._id));
    }

    private void HandleRemoved(MapNameId id)
    {
        if (_mapNames.TryRemove(id, out var _))
        {
            _luaEventHub.Broadcast(x => x.Remove(id._id));
        }
    }

    private void HandleAdded(MapNameId id, MapName mapName)
    {
        if(_mapNames.TryAdd(id, mapName))
        {
            _luaEventHub.Broadcast(x => x.Add(id._id, mapName.Name, mapName.Color, mapName.Position.X, mapName.Position.Y, mapName.Position.Z, mapName.Dimension, mapName.Interior, mapName.AttachedTo, mapName.Category, false));
        }
    }

    private void HandleAddedFor(MapNameId id, MapName mapName, Player[] players)
    {
        _luaEventHub.Invoke(players, x => x.Add(id._id, mapName.Name, mapName.Color, mapName.Position.X, mapName.Position.Y, mapName.Position.Z, mapName.Dimension, mapName.Interior, mapName.AttachedTo, mapName.Category, true));
    }

    private void HandleNameChanged(MapNameId id, string name)
    {
        _luaEventHub.Broadcast(x => x.SetName(id._id, name));
    }

    private void HandleNameChangedFor(MapNameId id, string name, Player[] players)
    {
        _luaEventHub.Invoke(players, x => x.SetName(id._id, name));
    }

    private async void HandlePlayerJoin(Player player)
    {
        try
        {
            await _resource.StartForAsync(player);
            foreach (var pair in _mapNames)
            {
                var mapName = pair.Value;
                _luaEventHub.Invoke(player, x => x.Add(pair.Key._id, mapName.Name, mapName.Color, mapName.Position.X, mapName.Position.Y, mapName.Position.Z, mapName.Dimension, mapName.Interior, mapName.AttachedTo, mapName.Category, false));
            }
        }
        catch(Exception ex)
        {
            _logger.ResourceFailedToStart<MapNamesResource>(ex);
        }
    }
}
