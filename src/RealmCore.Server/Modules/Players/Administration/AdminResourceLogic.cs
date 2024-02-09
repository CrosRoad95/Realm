namespace RealmCore.Server.Modules.Players.Administration;

internal sealed class AdminResourceLogic
{
    private readonly IElementFactory _elementFactory;
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminResourceLogic> _logger;
    private readonly ISpawnMarkersService _spawnMarkersService;
    private readonly IElementCollection _elementCollection;

    public AdminResourceLogic(IElementFactory elementFactory, IAdminService adminService, ILogger<AdminResourceLogic> logger, ISpawnMarkersService spawnMarkersService, IElementCollection elementCollection)
    {
        _elementFactory = elementFactory;
        _adminService = adminService;
        _logger = logger;
        _spawnMarkersService = spawnMarkersService;
        _elementCollection = elementCollection;
        _adminService.ToolStateChanged += HandleToolStateChanged;
    }

    private void HandleToolStateChanged(Player plr, AdminTool adminTool, bool state)
    {
        var player = (RealmPlayer)plr;
    using var _ = _logger.BeginElement(player);
        try
        {
            if (!player.Admin.HasTool(adminTool))
            {
                _logger.LogInformation("Player change admin tool {adminTool} state to {state} but tool is not allowed", adminTool, state);
                return;
            }
            _logger.LogInformation("Player change admin tool {adminTool} state to {state}", adminTool, state);
            InternalHandleToolStateChanged(player, adminTool, state);
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
        }
    }

    private void InternalHandleToolStateChanged(Player player, AdminTool adminTool, bool state)
    {
        switch (adminTool)
        {
            case AdminTool.Elements:
                if (state)
                {
                    List<ElementDebugInfo> debugInfoList = [];
                    foreach (var element in _elementCollection.GetAll())
                    {
                        debugInfoList.Add(new ElementDebugInfo
                        {
                            debugId = element.Id.ToString(),
                            element = element,
                            position = element.Position,
                            previewType = PreviewType.BoxWireframe,
                            previewColor = Color.Red,
                            name = element.GetType().ToString(),
                        });
                    }
                    _adminService.BroadcastElementDebugInfoUpdateForPlayer(player, debugInfoList);
                }
                else
                    _adminService.BroadcastClearElementsForPlayer(player);
                break;
            case AdminTool.ShowSpawnMarkers:
                if (state)
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
