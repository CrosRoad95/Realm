namespace RealmCore.Resources.Admin;

internal record struct AdminModeChangedMessage(Player Player, bool State) : IMessage;

internal record struct BroadcastElementDebugInfoMessage(ElementDebugInfo ElementDebugInfo) : IMessage;
internal record struct BroadcastElementsDebugInfoMessage(IEnumerable<ElementDebugInfo> ElementsDebugInfo) : IMessage;
internal record struct BroadcastElementDebugInfoMessageForPlayer(Player player, ElementDebugInfo ElementDebugInfo) : IMessage;
internal record struct BroadcastElementsDebugInfoMessageForPlayer(Player player, IEnumerable<ElementDebugInfo> ElementsDebugInfo) : IMessage;
internal record struct BroadcastSpawnMarkersForPlayerMessage(Player Player, IEnumerable<LuaValue> SpawnMarkers) : IMessage;
internal record struct ClearElementsForPlayerMessage(Player player) : IMessage;
internal record struct ClearSpawnMarkersForPlayerMessage(Player Player) : IMessage;
internal record struct SetAdminToolsMessage(Player Player, AdminTool[] AdminTools) : IMessage;
