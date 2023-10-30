using RealmCore.Resources.Admin.Data;

namespace RealmCore.Server.Logic.Resources;

internal sealed class AdminResourceLogic
{
    private readonly IEntityEngine _entityEngine;
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminResourceLogic> _logger;
    private readonly ISpawnMarkersService _spawnMarkersService;

    public AdminResourceLogic(IEntityEngine entityEngine, IAdminService adminService, ILogger<AdminResourceLogic> logger, ISpawnMarkersService spawnMarkersService)
    {
        _entityEngine = entityEngine;
        _adminService = adminService;
        _logger = logger;
        _spawnMarkersService = spawnMarkersService;

        _adminService.ToolStateChanged += HandleToolStateChanged;
    }

    private void HandleToolStateChanged(Player player, AdminTool adminTool, bool state)
    {
        using var _ = _logger.BeginElement(player);
        try
        {
            var entity = player.UpCast();
            using var _2 = _logger.BeginEntity(entity);
            if(!entity.TryGetComponent(out AdminComponent adminComponent))
            {
                _logger.LogInformation("Player change admin tool {adminTool} state to {state} but entity has no adminComponent", adminTool, state);
                return;
            }

            if(!adminComponent.HasAdminTool(adminTool))
            {
                _logger.LogInformation("Player change admin tool {adminTool} state to {state} but adminComponent is not enabled", adminTool, state);
                return;
            }
            _logger.LogInformation("Player change admin tool {adminTool} state to {state}", adminTool, state);
            InternalHandleToolStateChanged(player, adminTool, state);
        }
        catch(Exception ex)
        {
            _logger.LogHandleError(ex);
        }
    }

    private LuaValue GetDebugComponent(IComponent component)
    {
        var data = new Dictionary<LuaValue, LuaValue>
        {
            ["name"] = component.GetType().Name
        };
        if(component is ILuaDebugDataProvider luaDebugDataProvider)
        {
            data["data"] = luaDebugDataProvider.GetLuaDebugData();
        }

        return new LuaValue(data);
    }

    private List<LuaValue> GetDebugComponents(Entity entity)
    {
        return entity.Components
            .Select(GetDebugComponent)
            .ToList();
    }

    private void InternalHandleToolStateChanged(Player player, AdminTool adminTool, bool state)
    {
        switch (adminTool)
        {
            case AdminTool.Entities:
                if (state)
                {
                    List<EntityDebugInfo> debugInfoList = [];
                    foreach (var entity in _entityEngine.Entities)
                    {
                        if (entity.TryGetComponent(out IElementComponent elementComponent))
                        {
                            var element = (Element)elementComponent;
                            debugInfoList.Add(new EntityDebugInfo
                            {
                                debugId = entity.Id,
                                element = element,
                                position = element.Position,
                                previewType = PreviewType.BoxWireframe,
                                previewColor = Color.Red,
                                name = entity.GetType().ToString(),
                            });
                        }
                    }
                    _adminService.BroadcastEntityDebugInfoUpdateForPlayer(player, debugInfoList);
                }
                else
                    _adminService.BroadcastClearEntityForPlayer(player);
                break;
            case AdminTool.Components:
                if (state)
                {
                    var components = new Dictionary<LuaValue, LuaValue>();
                    foreach (var item in _entityEngine.Entities)
                    {
                        components[item.Id] = new LuaValue(GetDebugComponents(item));
                    }
                    _adminService.BroadcastEntitiesComponents(player, new LuaValue(components));
                } 
                else
                    _adminService.BroadcastClearEntitiesComponents(player);
                break;
            case AdminTool.ShowSpawnMarkers:
                if(state)
                {
                    _adminService.BroadcastSpawnMarkersForPlayer(player, _spawnMarkersService.SpawnMarkers.Select(x =>
                    {
                        switch (x)
                        {
                            case PointSpawnMarker pointSpawnMarker:
                                return new LuaValue(new Dictionary<LuaValue, LuaValue>
                                {
                                    ["type"] = "point",
                                    ["name"] = x.Name,
                                    ["position"] = LuaValue.ArrayFromVector(pointSpawnMarker.Position)
                                });
                            case DirectionalSpawnMarker directionalSpawnMarker:
                                return new LuaValue(new Dictionary<LuaValue, LuaValue>
                                {
                                    ["type"] = "directional",
                                    ["name"] = x.Name,
                                    ["position"] = LuaValue.ArrayFromVector(directionalSpawnMarker.Position),
                                    ["direction"] = new LuaValue(directionalSpawnMarker.Direction)
                                });
                            default:
                                throw new NotImplementedException();
                        }
                    }));
                }
                else
                    _adminService.BroadcastClearSpawnMarkersForPlayer(player);
                break;
        }
    }
}
