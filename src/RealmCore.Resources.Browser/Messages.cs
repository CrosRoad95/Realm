using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Browser;

internal record struct SetPathMessage(Player Player, string Path, bool Force) : IMessage;
internal record struct SetVisibleMessage(Player Player, bool Enabled) : IMessage;
internal record struct ToggleDevToolsMessage(Player Player, bool Enabled) : IMessage;