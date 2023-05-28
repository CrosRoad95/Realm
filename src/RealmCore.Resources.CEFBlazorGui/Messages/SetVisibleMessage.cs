using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.CEFBlazorGui.Messages;

internal record struct SetVisibleMessage(Player Player, bool Enabled) : IMessage;