using Realm.Domain.Components.Elements;
using Realm.Resources.AdminTools;
using SlipeServer.Resources.NoClip;
using SlipeServer.Server.Services;

namespace Realm.Domain.Components.Players;

public class AdminComponent : Component
{
    private bool _debugView = false;
    private bool _adminTools = false;
    private bool _noClip = false;

    public bool DebugView
    {
        get => _debugView; set
        {
            if (_debugView != value)
            {
                var player = Entity.InternalGetRequiredComponent<PlayerElementComponent>().Player;
                Entity.GetRequiredService<DebugLog>().SetVisibleTo(player, value);
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
