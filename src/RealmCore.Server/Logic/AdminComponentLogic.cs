namespace RealmCore.Console.Logic;

internal class AdminComponentLogic : ComponentLogic<AdminComponent>
{
    private readonly NoClipService _noClipService;
    private readonly DebugLog _debugLog;
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly IAdminService _adminService;

    public AdminComponentLogic(IECS ecs, NoClipService noClipService, DebugLog debugLog, IClientInterfaceService clientInterfaceService, IAdminService adminService) : base(ecs)
    {
        _noClipService = noClipService;
        _debugLog = debugLog;
        _clientInterfaceService = clientInterfaceService;
        _adminService = adminService;
    }

    private void HandleNoClipStateChanged(AdminComponent adminComponent, bool enabled)
    {
        _noClipService.SetEnabledTo(adminComponent.Entity.Player, enabled);
    }

    private void HandleDebugViewStateChanged(AdminComponent adminComponent, bool enabled)
    {
        _debugLog.SetVisibleTo(adminComponent.Entity.Player, enabled);
    }

    private void HandleDevelopmentModeStateChanged(AdminComponent adminComponent, bool enabled)
    {
        _clientInterfaceService.SetDevelopmentModeEnabled(adminComponent.Entity.Player, enabled);
    }

    private void HandleInteractionDebugRenderingStateChanged(AdminComponent adminComponent, bool enabled)
    {
        _clientInterfaceService.SetFocusableRenderingEnabled(adminComponent.Entity.Player, enabled);
    }

    private void AdminComponent_AdminModeChanged(AdminComponent adminComponent, bool enabled)
    {
        _adminService.SetAdminModeEnabledForPlayer(adminComponent.Entity.Player, enabled);
        if (enabled)
            _adminService.SetAdminTools(adminComponent.Entity.Player, adminComponent.AdminTools);

    }

    protected override void ComponentAdded(AdminComponent adminComponent)
    {
        adminComponent.NoClipStateChanged += HandleNoClipStateChanged;
        adminComponent.DebugViewStateChanged += HandleDebugViewStateChanged;
        adminComponent.DevelopmentModeStateChanged += HandleDevelopmentModeStateChanged;
        adminComponent.InteractionDebugRenderingStateChanged += HandleInteractionDebugRenderingStateChanged;
        adminComponent.AdminModeChanged += AdminComponent_AdminModeChanged;
    }

    protected override void ComponentRemoved(AdminComponent adminComponent)
    {
        adminComponent.NoClipStateChanged -= HandleNoClipStateChanged;
        adminComponent.DebugViewStateChanged -= HandleDebugViewStateChanged;
        adminComponent.DevelopmentModeStateChanged -= HandleDevelopmentModeStateChanged;
        adminComponent.InteractionDebugRenderingStateChanged -= HandleInteractionDebugRenderingStateChanged;
        adminComponent.AdminModeChanged -= AdminComponent_AdminModeChanged;
    }
}
