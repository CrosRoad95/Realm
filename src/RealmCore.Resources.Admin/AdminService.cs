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
}

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

    public void SetAdminTools(Player player, IEnumerable<AdminTool> adminTools)
    {
        MessageHandler?.Invoke(new SetAdminToolsMessage(player, adminTools.ToArray()));
    }

    public void BroadcastElementDebugInfoUpdate(ElementDebugInfo elementDebugInfo)
    {
        MessageHandler?.Invoke(new BroadcastElementDebugInfoMessage(elementDebugInfo));
    }

    public void BroadcastElementDebugInfoUpdate(IEnumerable<ElementDebugInfo> elementsDebugInfo)
    {
        MessageHandler?.Invoke(new BroadcastElementsDebugInfoMessage(elementsDebugInfo));
    }

    public void BroadcastElementDebugInfoUpdateForPlayer(Player player, ElementDebugInfo elementDebugInfo)
    {
        MessageHandler?.Invoke(new BroadcastElementDebugInfoMessageForPlayer(player, elementDebugInfo));
    }

    public void BroadcastElementDebugInfoUpdateForPlayer(Player player, IEnumerable<ElementDebugInfo> elementsDebugInfo)
    {
        MessageHandler?.Invoke(new BroadcastElementsDebugInfoMessageForPlayer(player, elementsDebugInfo));
    }

    public void BroadcastClearElementsForPlayer(Player player)
    {
        MessageHandler?.Invoke(new ClearElementsForPlayerMessage(player));
    }

    public void BroadcastSpawnMarkersForPlayer(Player player, IEnumerable<LuaValue> spawnMarkers)
    {
        MessageHandler?.Invoke(new BroadcastSpawnMarkersForPlayerMessage(player, spawnMarkers));
    }

    public void BroadcastClearSpawnMarkersForPlayer(Player player)
    {
        MessageHandler?.Invoke(new ClearSpawnMarkersForPlayerMessage(player));
    }
}
