namespace RealmCore.Resources.Admin.Messages;

internal record struct BroadcastSpawnMarkersForPlayerMessage(Player Player, IEnumerable<LuaValue> SpawnMarkers) : IMessage;
