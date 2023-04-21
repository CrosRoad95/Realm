namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class AdminComponent : Component
{
    [Inject]
    private NoClipService NoClipService { get; set; } = default!;
    [Inject]
    private DebugLog DebugLog { get; set; } = default!;
    [Inject]
    private IAdminService AdminService { get; set; } = default!;
    [Inject]
    private IClientInterfaceService ClientInterfaceService { get; set; } = default!;

    private bool _debugView = false;
    private bool _adminMode = false;
    private bool _noClip = false;
    private bool _developmentMode = false;
    private bool _interactionDebugRenderingEnabled = false;

    public event Action<AdminComponent, bool>? DebugViewStateChanged;
    public event Action<AdminComponent, bool>? AdminModeChanged;
    public event Action<AdminComponent, bool>? NoClipStateChanged;
    public event Action<AdminComponent, bool>? DevelopmentModeStateChanged;
    public event Action<AdminComponent, bool>? InteractionDebugRenderingStateChanged;

    public bool DevelopmentMode
    {
        get
        {
            ThrowIfDisposed();
            return _developmentMode;
        }
        set
        {
            ThrowIfDisposed();
            if (_developmentMode != value)
            {
                ClientInterfaceService.SetDevelopmentModeEnabled(Entity.Player, value);
                _developmentMode = value;
                DevelopmentModeStateChanged?.Invoke(this, _developmentMode);
            }
        }
    }

    public bool DebugView
    {
        get
        {
            ThrowIfDisposed();
            return _debugView;
        }
        set
        {
            ThrowIfDisposed();
            if (_debugView != value)
            {
                DebugLog.SetVisibleTo(Entity.Player, value);
                _debugView = value;
                DebugViewStateChanged?.Invoke(this, _debugView);
            }
        }
    }

    public bool HasAdminModeEnabled
    {
        get
        {
            ThrowIfDisposed();
            return _adminMode;
        }
        set
        {
            ThrowIfDisposed();
            if (_adminMode != value)
            {
                if (value)
                {
                    AdminService.EnableAdminModeForPlayer(Entity.Player);
                }
                else
                {
                    AdminService.DisableAdminModeForPlayer(Entity.Player);
                }
                _adminMode = value;
                AdminModeChanged?.Invoke(this, _adminMode);
            }
        }
    }

    public bool NoClip
    {
        get
        {
            ThrowIfDisposed();
            return _noClip;
        }
        set
        {
            ThrowIfDisposed();
            if (_noClip != value)
            {
                NoClipService.SetEnabledTo(Entity.Player, value);
                _noClip = value;
                NoClipStateChanged?.Invoke(this, _noClip);
            }
        }
    }

    public bool InteractionDebugRenderingEnabled
    {
        get
        {
            ThrowIfDisposed();
            return _interactionDebugRenderingEnabled;
        }
        set
        {
            ThrowIfDisposed();
            if (_interactionDebugRenderingEnabled != value)
            {
                ClientInterfaceService.SetFocusableRenderingEnabled(Entity.Player, _interactionDebugRenderingEnabled);
                _interactionDebugRenderingEnabled = value;
                NoClipStateChanged?.Invoke(this, _interactionDebugRenderingEnabled);

            }
        }
    }

    public override void Dispose()
    {
        DevelopmentMode = false;
        DebugView = false;
        HasAdminModeEnabled = false;
        NoClip = false;
        InteractionDebugRenderingEnabled = false;
        base.Dispose();
    }
}
