using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.CEFBlazorGui;

internal record struct SetPathMessage(Player Player, string Path, bool Force, bool IsAsync) : IMessage;
internal record struct SetVisibleMessage(Player Player, bool Enabled) : IMessage;
internal record struct ToggleDevToolsMessage(Player Player, bool Enabled) : IMessage;
internal record struct SetRemotePathMessage(Player Player, string Path) : IMessage;