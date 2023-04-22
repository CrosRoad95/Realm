using RealmCore.Resources.Admin.Data;
using RealmCore.Resources.Admin.Enums;

namespace RealmCore.Server.Logic.Resources;

internal sealed class AdminResourceLogic
{
    private readonly IECS _ecs;
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminResourceLogic> _logger;

    public AdminResourceLogic(IECS ecs, IAdminService adminService, ILogger<AdminResourceLogic> logger)
    {
        _ecs = ecs;
        _adminService = adminService;
        _logger = logger;
        _ecs.EntityCreated += HandleEntityCreated;

        _adminService.ToolStateChanged += HandleToolStateChanged;
    }

    private void HandleToolStateChanged(Player player, AdminTool adminTool, bool state)
    {
        using var _ = _logger.BeginPlayer(player);
        try
        {
            if(!_ecs.TryGetEntityByPlayer(player, out var entity))
            {
                _logger.LogWarning("Player attempted to change admin tool {adminTool} state to {state} but entity was not found", adminTool, state);
                return;
            }
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

    private void InternalHandleToolStateChanged(Player player, AdminTool adminTool, bool state)
    {
        switch (adminTool)
        {
            case AdminTool.Entities:
                if (state)
                    _adminService.BroadcastEntityDebugInfoUpdateForPlayer(player, _ecs.Entities.Select(x => new EntityDebugInfo
                    {
                        debugId = x.Id,
                        element = x.TryGetElement(),
                        position = x.Transform.Position,
                        previewType = PreviewType.BoxWireframe,
                        previewColor = Color.Red,
                        name = x.Name,
                        tag = x.Tag.ToString(),
                    }));
                else
                    _adminService.BroadcastClearEntityForPlayer(player);
                break;
        }
    }

    private void HandleEntityCreated(Entity entity)
    {
        entity.Disposed += HandleDisposed;
        entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is not AdminComponent adminComponent)
            return;

    }

    private void HandleDisposed(Entity entity)
    {
        entity.Disposed -= HandleDisposed;
        entity.ComponentAdded -= HandleComponentAdded;
    }
}
