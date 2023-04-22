using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Admin.Messages;

internal record struct AdminModeChangedMessage(Player Player, bool State) : IMessage;
