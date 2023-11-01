using RealmCore.Resources.Admin.Data;
using RealmCore.Resources.Admin.Enums;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Admin;

public interface IAdminService
{
    internal Action<IMessage>? MessageHandler { get; set; }

    event Action<Player, AdminTool, bool>? ToolStateChanged;

    internal void RelayToolStateChanged(Player player, AdminTool adminTool, bool state);
    void SetAdminModeEnabledForPlayer(Player player, bool enabled);
    void SetAdminTools(Player player, IEnumerable<AdminTool> adminTools);
    void BroadcastElementDebugInfoUpdate(ElementDebugInfo elementDebugInfo);
    void BroadcastElementDebugInfoUpdate(IEnumerable<ElementDebugInfo> elementDebugInfo);
    void BroadcastElementDebugInfoUpdateForPlayer(Player player, ElementDebugInfo elementDebugInfo);
    void BroadcastElementDebugInfoUpdateForPlayer(Player player, IEnumerable<ElementDebugInfo> elementsDebugInfo);
    void BroadcastClearElementsForPlayer(Player player);
    void BroadcastSpawnMarkersForPlayer(Player player, IEnumerable<LuaValue> elementsDebugInfo);
    void BroadcastClearSpawnMarkersForPlayer(Player player);
    void BroadcastElementsComponents(Player player, LuaValue elementsComponents);
    void BroadcastClearElementsComponents(Player player);
}
