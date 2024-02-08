namespace RealmCore.Server.Services.Players;

public interface IPlayerAdminService : IPlayerService
{
    IReadOnlyList<AdminTool> AdminTools { get; }
    bool DevelopmentMode { get; set; }
    bool DebugView { get; set; }
    bool AdminMode { get; set; }
    bool NoClip { get; set; }
    bool InteractionDebugRenderingEnabled { get; set; }

    event Action<IPlayerAdminService, bool>? DebugViewStateChanged;
    event Action<IPlayerAdminService, bool>? AdminModeChanged;
    event Action<IPlayerAdminService, bool>? NoClipStateChanged;
    event Action<IPlayerAdminService, bool>? DevelopmentModeStateChanged;
    event Action<IPlayerAdminService, bool>? InteractionDebugRenderingStateChanged;

    bool HasTool(AdminTool adminTool);
    void SetTools(IEnumerable<AdminTool> adminTools);
}

internal sealed class PlayerAdminService : IPlayerAdminService
{
    private readonly object _lock = new();
    private bool _debugView = false;
    private bool _adminMode = false;
    private bool _noClip = false;
    private bool _developmentMode = false;
    private bool _interactionDebugRenderingEnabled = false;
    private List<AdminTool> _adminTools = [];

    public event Action<IPlayerAdminService, bool>? DebugViewStateChanged;
    public event Action<IPlayerAdminService, bool>? AdminModeChanged;
    public event Action<IPlayerAdminService, bool>? NoClipStateChanged;
    public event Action<IPlayerAdminService, bool>? DevelopmentModeStateChanged;
    public event Action<IPlayerAdminService, bool>? InteractionDebugRenderingStateChanged;

    public IReadOnlyList<AdminTool> AdminTools
    {
        get
        {
            lock (_lock)
                return new List<AdminTool>(_adminTools);
        }
    }
    public bool DevelopmentMode
    {
        get
        {
            return _developmentMode;
        }
        set
        {
            if (_developmentMode != value)
            {
                _developmentMode = value;
                DevelopmentModeStateChanged?.Invoke(this, _developmentMode);
            }
        }
    }

    public bool DebugView
    {
        get
        {
            return _debugView;
        }
        set
        {
            if (_debugView != value)
            {
                _debugView = value;
                DebugViewStateChanged?.Invoke(this, _debugView);
            }
        }
    }

    public bool AdminMode
    {
        get
        {
            return _adminMode;
        }
        set
        {
            if (_adminMode != value)
            {
                _adminMode = value;
                AdminModeChanged?.Invoke(this, _adminMode);
            }
        }
    }

    public bool NoClip
    {
        get
        {
            return _noClip;
        }
        set
        {
            if (_noClip != value)
            {
                _noClip = value;
                NoClipStateChanged?.Invoke(this, _noClip);
            }
        }
    }

    public bool InteractionDebugRenderingEnabled
    {
        get
        {
            return _interactionDebugRenderingEnabled;
        }
        set
        {
            if (_interactionDebugRenderingEnabled != value)
            {
                _interactionDebugRenderingEnabled = value;
                InteractionDebugRenderingStateChanged?.Invoke(this, _interactionDebugRenderingEnabled);
            }
        }
    }

    public RealmPlayer Player { get; init; }
    public PlayerAdminService(PlayerContext playerContext, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
    }

    public void SetTools(IEnumerable<AdminTool> adminTools)
    {
        lock (_lock)
        {
            _adminTools = new(adminTools);
        }
    }

    public bool HasTool(AdminTool adminTool)
    {
        lock (_lock)
        {
            return _adminTools.Contains(adminTool);
        }
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {

    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
    {

    }
}
