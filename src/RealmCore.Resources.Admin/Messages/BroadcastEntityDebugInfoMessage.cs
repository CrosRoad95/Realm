using RealmCore.Resources.Admin.Data;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Admin.Messages;

internal record struct BroadcastEntityDebugInfoMessage(EntityDebugInfo EntityDebugInfo) : IMessage;
internal record struct BroadcastEntitiesDebugInfoMessage(IEnumerable<EntityDebugInfo> EntitiesDebugInfo) : IMessage;
internal record struct BroadcastEntityDebugInfoMessageForPlayer(Player player, EntityDebugInfo EntityDebugInfo) : IMessage;
internal record struct BroadcastEntitiesDebugInfoMessageForPlayer(Player player, IEnumerable<EntityDebugInfo> EntitiesDebugInfo) : IMessage;
