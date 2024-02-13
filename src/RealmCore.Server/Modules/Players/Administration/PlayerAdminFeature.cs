namespace RealmCore.Server.Modules.Players.Administration;

public interface IPlayerAdminFeature : IPlayerFeature
{
    IReadOnlyList<AdminTool> Tools { get; }
    bool DevelopmentMode { get; set; }
    bool DebugView { get; set; }
    bool AdminMode { get; set; }
    bool NoClip { get; set; }
    bool InteractionDebugRenderingEnabled { get; set; }

    event Action<IPlayerAdminFeature, bool>? DebugViewStateChanged;
    event Action<IPlayerAdminFeature, bool>? AdminModeChanged;
    event Action<IPlayerAdminFeature, bool>? NoClipStateChanged;
    event Action<IPlayerAdminFeature, bool>? DevelopmentModeStateChanged;
    event Action<IPlayerAdminFeature, bool>? InteractionDebugRenderingStateChanged;

    bool HasTool(AdminTool adminTool);
    void SetTools(IEnumerable<AdminTool> adminTools);
    void Reset();
}

internal sealed class PlayerAdminFeature : IPlayerAdminFeature, IDisposable
{
    private readonly object _lock = new();
    private readonly IPlayerUserFeature _playerUserFeature;
    private bool _debugView = false;
    private bool _adminMode = false;
    private bool _noClip = false;
    private bool _developmentMode = false;
    private bool _interactionDebugRenderingEnabled = false;
    private List<AdminTool> _tools = [];

    public event Action<IPlayerAdminFeature, IReadOnlyList<AdminTool>>? ToolsChanged;
    public event Action<IPlayerAdminFeature, bool>? DebugViewStateChanged;
    public event Action<IPlayerAdminFeature, bool>? AdminModeChanged;
    public event Action<IPlayerAdminFeature, bool>? NoClipStateChanged;
    public event Action<IPlayerAdminFeature, bool>? DevelopmentModeStateChanged;
    public event Action<IPlayerAdminFeature, bool>? InteractionDebugRenderingStateChanged;

    public IReadOnlyList<AdminTool> Tools
    {
        get
        {
            lock (_lock)
                return new List<AdminTool>(_tools);
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

    public PlayerAdminFeature(PlayerContext playerContext, IPlayerUserFeature playerUserFeature)
    {
        _playerUserFeature = playerUserFeature;
        Player = playerContext.Player;
        playerUserFeature.SignedOut += HandleSignedOut;
    }

    public void SetTools(IEnumerable<AdminTool> adminTools)
    {
        lock (_lock)
        {
            _tools = new(adminTools);
            ToolsChanged?.Invoke(this, _tools);
        }
    }

    public bool HasTool(AdminTool adminTool)
    {
        lock (_lock)
        {
            return _tools.Contains(adminTool);
        }
    }

    public void Reset()
    {
        DevelopmentMode = false;
        DebugView = false;
        AdminMode = false;
        NoClip = false;
        InteractionDebugRenderingEnabled = false;
        SetTools([]);
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        Reset();
    }

    public void Dispose()
    {
        _playerUserFeature.SignedOut -= HandleSignedOut;
    }
}
