using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Admin.Messages;

internal record struct BroadcastSpawnMarkersForPlayerMessage(Player Player, IEnumerable<LuaValue> SpawnMarkers) : IMessage;
