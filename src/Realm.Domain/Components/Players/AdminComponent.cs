using Realm.Domain.Components.Elements;
using Realm.Resources.AdminTools;
using SlipeServer.Resources.NoClip;

namespace Realm.Domain.Components.Players;

public class AdminComponent : Component
{
    private bool _debugView = false;
    public bool DebugView
    {
        get => _debugView; set
        {
            if (_debugView != value)
            {
                _debugView = value;
            }
        }
    }

    private bool _adminTools = false;
    public bool AdminTools
    {
        get => _adminTools; set
        {
            if (_adminTools != value)
            {
                _adminTools = value;
                var player = Entity.InternalGetRequiredComponent<PlayerElementComponent>().Player;
                if (value)
                {
                    Entity.GetRequiredService<AdminToolsService>().EnableAdminToolsForPlayer(player);
                }
                else
                {
                    Entity.GetRequiredService<AdminToolsService>().DisableAdminToolsForPlayer(player);
                }
            }
        }
    }

    private bool _noClip = false;
    public bool NoClip
    {
        get => _noClip; set
        {
            if (_noClip != value)
            {
                _noClip = value;
                var player = Entity.InternalGetRequiredComponent<PlayerElementComponent>().Player;
                Entity.GetRequiredService<NoClipService>().SetEnabledTo(player, value);
            }
        }
    }

}
