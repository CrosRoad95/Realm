using RealmCore.Resources.Admin.Data;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Admin.Messages;

internal record struct BroadcastElementDebugInfoMessage(ElementDebugInfo ElementDebugInfo) : IMessage;
internal record struct BroadcastElementsDebugInfoMessage(IEnumerable<ElementDebugInfo> ElementsDebugInfo) : IMessage;
internal record struct BroadcastElementDebugInfoMessageForPlayer(Player player, ElementDebugInfo ElementDebugInfo) : IMessage;
internal record struct BroadcastElementsDebugInfoMessageForPlayer(Player player, IEnumerable<ElementDebugInfo> ElementsDebugInfo) : IMessage;
