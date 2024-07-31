namespace RealmCore.Server.Modules.Players.Administration;

internal sealed class AdministrationHostedService : PlayerLifecycle, IHostedService
{
    private readonly NoClipService _noClipService;
    private readonly DebugLog _debugLog;
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly IAdminService _adminService;

    public AdministrationHostedService(PlayersEventManager playersEventManager, NoClipService noClipService, DebugLog debugLog, IClientInterfaceService clientInterfaceService, IAdminService adminService) : base(playersEventManager)
    {
        _noClipService = noClipService;
        _debugLog = debugLog;
        _clientInterfaceService = clientInterfaceService;
        _adminService = adminService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
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

    private void HandleNoClipStateChanged(PlayerAdminFeature admin, bool enabled)
    {
        _noClipService.SetEnabledTo(admin.Player, enabled);
    }

    private void HandleDebugViewStateChanged(PlayerAdminFeature admin, bool enabled)
    {
        _debugLog.SetVisibleTo(admin.Player, enabled);
    }

    private void HandleDevelopmentModeStateChanged(PlayerAdminFeature admin, bool enabled)
    {
        _clientInterfaceService.SetDevelopmentModeEnabled(admin.Player, enabled);
    }

    private void HandleInteractionDebugRenderingStateChanged(PlayerAdminFeature admin, bool enabled)
    {
        _clientInterfaceService.SetFocusableRenderingEnabled(admin.Player, enabled);
    }

    private void HandleAdminModeChanged(PlayerAdminFeature admin, bool enabled)
    {
        _adminService.SetAdminModeEnabledForPlayer(admin.Player, enabled);
        if (enabled)
            _adminService.SetAdminTools(admin.Player, admin.Tools);
    }
}
