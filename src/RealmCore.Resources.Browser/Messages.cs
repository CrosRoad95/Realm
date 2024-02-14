using RealmCore.Resources.Base.Interfaces;

namespace RealmCore.Resources.Browser;

internal record struct SetPathMessage(Player Player, string Path) : IMessage;
internal record struct SetVisibleMessage(Player Player, bool Enabled) : IMessage;
internal record struct ToggleDevToolsMessage(Player Player, bool Enabled) : IMessage;