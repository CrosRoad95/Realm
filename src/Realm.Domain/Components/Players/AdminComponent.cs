using Realm.Resources.AdminTools;
using SlipeServer.Resources.NoClip;

namespace Realm.Domain.Components.Players;

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

    public bool DevelopmentMode
    {
        get => _developmentMode; set
        {
            if (_developmentMode != value)
            {
                var player = Entity.GetRequiredComponent<PlayerElementComponent>().Player;
                ClientInterfaceService.SetDevelopmentModeEnabled(player, value);
                _developmentMode = value;
            }
        }
    }
    
    public bool DebugView
    {
        get => _debugView; set
        {
            if (_debugView != value)
            {
                var player = Entity.GetRequiredComponent<PlayerElementComponent>().Player;
                DebugLog.SetVisibleTo(player, value);
                _debugView = value;
            }
        }
    }

    public bool AdminTools
    {
        get => _adminTools; set
        {
            if (_adminTools != value)
            {
                _adminTools = value;
                var player = Entity.GetRequiredComponent<PlayerElementComponent>().Player;
                if (value)
                {
                    AdminToolsService.EnableAdminToolsForPlayer(player);
                }
                else
                {
                    AdminToolsService.DisableAdminToolsForPlayer(player);
                }
            }
        }
    }

    public bool NoClip
    {
        get => _noClip; set
        {
            if (_noClip != value)
            {
                _noClip = value;
                var player = Entity.GetRequiredComponent<PlayerElementComponent>().Player;
                NoClipService.SetEnabledTo(player, value);
            }
        }
    }
}
