using RealmCore.Resources.Admin.Data;
using RealmCore.Resources.Admin.Enums;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Admin;

public interface IAdminService
{
    internal Action<IMessage>? MessageHandler { get; set; }

    event Action<Player, AdminTool, bool>? ToolStateChanged;

    internal void RelayToolStateChanged(Player player, AdminTool adminTool, bool state);
    void SetAdminModeEnabledForPlayer(Player player, bool enabled);
    void SetAdminTools(Player player, IReadOnlyList<AdminTool> adminTools);
    void BroadcastEntityDebugInfoUpdate(EntityDebugInfo entityDebugInfo);
    void BroadcastEntityDebugInfoUpdate(IEnumerable<EntityDebugInfo> entityDebugInfo);
    void BroadcastEntityDebugInfoUpdateForPlayer(Player player, EntityDebugInfo entityDebugInfo);
    void BroadcastEntityDebugInfoUpdateForPlayer(Player player, IEnumerable<EntityDebugInfo> entitiesDebugInfo);
    void BroadcastClearEntityForPlayer(Player player);
}
