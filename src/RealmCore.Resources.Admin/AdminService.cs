using RealmCore.Resources.Admin.Data;
using RealmCore.Resources.Admin.Enums;
using RealmCore.Resources.Admin.Messages;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Admin;

internal sealed class AdminService : IAdminService
{
    public Action<IMessage>? MessageHandler { get; set; }

    public event Action<Player, AdminTool, bool>? ToolStateChanged;
    public AdminService()
    {

    }

    public void RelayToolStateChanged(Player player, AdminTool adminTool, bool state)
    {
        ToolStateChanged?.Invoke(player, adminTool, state);
    }

    public void SetAdminModeEnabledForPlayer(Player player, bool enabled)
    {
        MessageHandler?.Invoke(new AdminModeChangedMessage(player, enabled));
    }

    public void SetAdminTools(Player player, IReadOnlyList<AdminTool> adminTools)
    {
        MessageHandler?.Invoke(new SetAdminToolsMessage(player, adminTools));
    }

    public void BroadcastEntityDebugInfoUpdate(EntityDebugInfo entityDebugInfo)
    {
        MessageHandler?.Invoke(new BroadcastEntityDebugInfoMessage(entityDebugInfo));
    }

    public void BroadcastEntityDebugInfoUpdate(IEnumerable<EntityDebugInfo> entitiesDebugInfo)
    {
        MessageHandler?.Invoke(new BroadcastEntitiesDebugInfoMessage(entitiesDebugInfo));
    }

    public void BroadcastEntityDebugInfoUpdateForPlayer(Player player, EntityDebugInfo entityDebugInfo)
    {
        MessageHandler?.Invoke(new BroadcastEntityDebugInfoMessageForPlayer(player, entityDebugInfo));
    }

    public void BroadcastEntityDebugInfoUpdateForPlayer(Player player, IEnumerable<EntityDebugInfo> entitiesDebugInfo)
    {
        MessageHandler?.Invoke(new BroadcastEntitiesDebugInfoMessageForPlayer(player, entitiesDebugInfo));
    }

    public void BroadcastClearEntityForPlayer(Player player)
    {
        MessageHandler?.Invoke(new ClearEntitiesForPlayerMessage(player));
    }

    public void BroadcastSpawnMarkersForPlayer(Player player, IEnumerable<LuaValue> spawnMarkers)
    {
        MessageHandler?.Invoke(new BroadcastSpawnMarkersForPlayerMessage(player, spawnMarkers));
    }

    public void BroadcastClearSpawnMarkersForPlayer(Player player)
    {
        MessageHandler?.Invoke(new ClearEntitiesForPlayerMessage(player));
    }
}
