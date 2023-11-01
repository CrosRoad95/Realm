namespace RealmCore.Server.Logic.Components;

internal sealed class AdminComponentLogic : ComponentLogic<AdminComponent>
{
    private readonly NoClipService _noClipService;
    private readonly DebugLog _debugLog;
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly IAdminService _adminService;

    public AdminComponentLogic(IElementFactory elementFactory, NoClipService noClipService, DebugLog debugLog, IClientInterfaceService clientInterfaceService, IAdminService adminService) : base(elementFactory)
    {
        _noClipService = noClipService;
        _debugLog = debugLog;
        _clientInterfaceService = clientInterfaceService;
        _adminService = adminService;
    }

    private void HandleNoClipStateChanged(AdminComponent adminComponent, bool enabled)
    {
        _noClipService.SetEnabledTo((RealmPlayer)adminComponent.Element, enabled);
    }

    private void HandleDebugViewStateChanged(AdminComponent adminComponent, bool enabled)
    {
        _debugLog.SetVisibleTo((RealmPlayer)adminComponent.Element, enabled);
    }

    private void HandleDevelopmentModeStateChanged(AdminComponent adminComponent, bool enabled)
    {
        _clientInterfaceService.SetDevelopmentModeEnabled((RealmPlayer)adminComponent.Element, enabled);
    }

    private void HandleInteractionDebugRenderingStateChanged(AdminComponent adminComponent, bool enabled)
    {
        _clientInterfaceService.SetFocusableRenderingEnabled((RealmPlayer)adminComponent.Element, enabled);
    }

    private void AdminComponent_AdminModeChanged(AdminComponent adminComponent, bool enabled)
    {
        _adminService.SetAdminModeEnabledForPlayer((RealmPlayer)adminComponent.Element, enabled);
        if (enabled)
            _adminService.SetAdminTools((RealmPlayer)adminComponent.Element, adminComponent.AdminTools);

    }

    protected override void ComponentAdded(AdminComponent adminComponent)
    {
        adminComponent.NoClipStateChanged += HandleNoClipStateChanged;
        adminComponent.DebugViewStateChanged += HandleDebugViewStateChanged;
        adminComponent.DevelopmentModeStateChanged += HandleDevelopmentModeStateChanged;
        adminComponent.InteractionDebugRenderingStateChanged += HandleInteractionDebugRenderingStateChanged;
        adminComponent.AdminModeChanged += AdminComponent_AdminModeChanged;
    }

    protected override void ComponentDetached(AdminComponent adminComponent)
    {
        adminComponent.NoClipStateChanged -= HandleNoClipStateChanged;
        adminComponent.DebugViewStateChanged -= HandleDebugViewStateChanged;
        adminComponent.DevelopmentModeStateChanged -= HandleDevelopmentModeStateChanged;
        adminComponent.InteractionDebugRenderingStateChanged -= HandleInteractionDebugRenderingStateChanged;
        adminComponent.AdminModeChanged -= AdminComponent_AdminModeChanged;
    }
}
