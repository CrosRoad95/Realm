using Realm.Resources.AdminTools;
using SlipeServer.Resources.NoClip;

namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class AdminComponent : Component
{
    [Inject]
    private NoClipService NoClipService { get; set; } = default!;
    [Inject]
    private DebugLog DebugLog { get; set; } = default!;
    [Inject]
    private AdminToolsService AdminToolsService { get; set; } = default!;
    [Inject]
    private ClientInterfaceService ClientInterfaceService { get; set; } = default!;

    private bool _debugView = false;
    private bool _adminTools = false;
    private bool _noClip = false;
    private bool _developmentMode = false;
    private bool _interactionDebugRenderingEnabled = false;

    public event Action<AdminComponent, bool>? DebugViewStateChanged;
    public event Action<AdminComponent, bool>? AdminToolsStateChanged;
    public event Action<AdminComponent, bool>? NoClipStateChanged;
    public event Action<AdminComponent, bool>? DevelopmentModeStateChanged;
    public event Action<AdminComponent, bool>? InteractionDebugRenderingStateChanged;

    public bool DevelopmentMode
    {
        get => _developmentMode; set
        {
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
        get => _debugView; set
        {
            if (_debugView != value)
            {
                DebugLog.SetVisibleTo(Entity.Player, value);
                _debugView = value;
                DebugViewStateChanged?.Invoke(this, _debugView);
            }
        }
    }

    public bool AdminTools
    {
        get => _adminTools; set
        {
            if (_adminTools != value)
            {
                if (value)
                {
                    AdminToolsService.EnableAdminToolsForPlayer(Entity.Player);
                }
                else
                {
                    AdminToolsService.DisableAdminToolsForPlayer(Entity.Player);
                }
                _adminTools = value;
                AdminToolsStateChanged?.Invoke(this, _adminTools);
            }
        }
    }

    public bool NoClip
    {
        get => _noClip; set
        {
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
        get => _interactionDebugRenderingEnabled; set
        {
            if (_interactionDebugRenderingEnabled != value)
            {
                ClientInterfaceService.SetFocusableRenderingEnabled(Entity.Player, _interactionDebugRenderingEnabled);
                _interactionDebugRenderingEnabled = value;
                NoClipStateChanged?.Invoke(this, _interactionDebugRenderingEnabled);

            }
        }
    }

    public void ToggleNoClip()
    {
        NoClip = !NoClip;
    }

    public override void Dispose()
    {
        DevelopmentMode = false;
        DebugView = false;
        AdminTools = false;
        NoClip = false;
        InteractionDebugRenderingEnabled = false;
    }
}
