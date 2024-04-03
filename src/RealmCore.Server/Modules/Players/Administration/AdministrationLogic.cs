namespace RealmCore.Server.Modules.Players.Administration;

internal sealed class AdministrationLogic : PlayerLifecycle
{
    private readonly NoClipService _noClipService;
    private readonly DebugLog _debugLog;
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly IAdminService _adminService;

    public AdministrationLogic(PlayersEventManager playersEventManager, NoClipService noClipService, DebugLog debugLog, IClientInterfaceService clientInterfaceService, IAdminService adminService) : base(playersEventManager)
    {
        _noClipService = noClipService;
        _debugLog = debugLog;
        _clientInterfaceService = clientInterfaceService;
        _adminService = adminService;
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        var admin = player.Admin;

        admin.NoClipStateChanged += HandleNoClipStateChanged;
        admin.DebugViewStateChanged += HandleDebugViewStateChanged;
        admin.DevelopmentModeStateChanged += HandleDevelopmentModeStateChanged;
        admin.InteractionDebugRenderingStateChanged += HandleInteractionDebugRenderingStateChanged;
        admin.AdminModeChanged += HandleAdminModeChanged;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        var admin = player.Admin;

        admin.NoClipStateChanged -= HandleNoClipStateChanged;
        admin.DebugViewStateChanged -= HandleDebugViewStateChanged;
        admin.DevelopmentModeStateChanged -= HandleDevelopmentModeStateChanged;
        admin.InteractionDebugRenderingStateChanged -= HandleInteractionDebugRenderingStateChanged;
        admin.AdminModeChanged -= HandleAdminModeChanged;
    }

    private void HandleNoClipStateChanged(IPlayerAdminFeature admin, bool enabled)
    {
        _noClipService.SetEnabledTo(admin.Player, enabled);
    }

    private void HandleDebugViewStateChanged(IPlayerAdminFeature admin, bool enabled)
    {
        _debugLog.SetVisibleTo(admin.Player, enabled);
    }

    private void HandleDevelopmentModeStateChanged(IPlayerAdminFeature admin, bool enabled)
    {
        _clientInterfaceService.SetDevelopmentModeEnabled(admin.Player, enabled);
    }

    private void HandleInteractionDebugRenderingStateChanged(IPlayerAdminFeature admin, bool enabled)
    {
        _clientInterfaceService.SetFocusableRenderingEnabled(admin.Player, enabled);
    }

    private void HandleAdminModeChanged(IPlayerAdminFeature admin, bool enabled)
    {
        _adminService.SetAdminModeEnabledForPlayer(admin.Player, enabled);
        if (enabled)
            _adminService.SetAdminTools(admin.Player, admin.Tools);
    }
}
