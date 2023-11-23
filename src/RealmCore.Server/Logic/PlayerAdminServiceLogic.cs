namespace RealmCore.Server.Logic;

internal sealed class PlayerAdminServiceLogic
{
    private readonly NoClipService _noClipService;
    private readonly DebugLog _debugLog;
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly IAdminService _adminService;

    public PlayerAdminServiceLogic(MtaServer mtaServer, NoClipService noClipService, DebugLog debugLog, IClientInterfaceService clientInterfaceService, IAdminService adminService)
    {
        _noClipService = noClipService;
        _debugLog = debugLog;
        _clientInterfaceService = clientInterfaceService;
        _adminService = adminService;
        mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player plr)
    {
        var player = (RealmPlayer)plr;
        var admin = player.Admin;

        admin.NoClipStateChanged += HandleNoClipStateChanged;
        admin.DebugViewStateChanged += HandleDebugViewStateChanged;
        admin.DevelopmentModeStateChanged += HandleDevelopmentModeStateChanged;
        admin.InteractionDebugRenderingStateChanged += HandleInteractionDebugRenderingStateChanged;
        admin.AdminModeChanged += HandleAdminModeChanged;
    }

    private void HandleNoClipStateChanged(IPlayerAdminService admin, bool enabled)
    {
        _noClipService.SetEnabledTo(admin.Player, enabled);
    }

    private void HandleDebugViewStateChanged(IPlayerAdminService admin, bool enabled)
    {
        _debugLog.SetVisibleTo(admin.Player, enabled);
    }

    private void HandleDevelopmentModeStateChanged(IPlayerAdminService admin, bool enabled)
    {
        _clientInterfaceService.SetDevelopmentModeEnabled(admin.Player, enabled);
    }

    private void HandleInteractionDebugRenderingStateChanged(IPlayerAdminService admin, bool enabled)
    {
        _clientInterfaceService.SetFocusableRenderingEnabled(admin.Player, enabled);
    }

    private void HandleAdminModeChanged(IPlayerAdminService admin, bool enabled)
    {
        _adminService.SetAdminModeEnabledForPlayer(admin.Player, enabled);
        if (enabled)
            _adminService.SetAdminTools(admin.Player, admin.AdminTools);

    }
}
