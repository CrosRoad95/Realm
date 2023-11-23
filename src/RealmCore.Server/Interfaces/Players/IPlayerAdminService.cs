

namespace RealmCore.Server.Interfaces.Players;

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
