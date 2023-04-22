using RealmCore.Resources.Admin.Enums;

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
    private readonly List<AdminTool> _adminTools;

    public event Action<AdminComponent, bool>? DebugViewStateChanged;
    public event Action<AdminComponent, bool>? AdminModeChanged;
    public event Action<AdminComponent, bool>? NoClipStateChanged;
    public event Action<AdminComponent, bool>? DevelopmentModeStateChanged;
    public event Action<AdminComponent, bool>? InteractionDebugRenderingStateChanged;

    public AdminComponent(List<AdminTool> adminTools)
    {
        _adminTools = adminTools;
    }

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

    public bool AdminMode
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
                _adminMode = value;
                AdminService.SetAdminModeEnabledForPlayer(Entity.Player, _adminMode);
                if(value)
                    AdminService.SetAdminTools(Entity.Player, _adminTools.AsReadOnly());

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
        AdminMode = false;
        NoClip = false;
        InteractionDebugRenderingEnabled = false;
        base.Dispose();
    }
}
