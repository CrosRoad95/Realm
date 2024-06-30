using RealmCore.Resources.Base.Interfaces;
using System.Numerics;

namespace RealmCore.Resources.Browser;

internal record struct SetPathMessage(Player Player, string Path) : IMessage;
internal record struct SetVisibleMessage(Player Player, bool Enabled) : IMessage;
internal record struct ToggleDevToolsMessage(Player Player, bool Enabled) : IMessage;
internal record struct LoadBrowser(Player Player, Vector2 Size, string RemoteUrl, string RequestWhitelistUrl) : IMessage;