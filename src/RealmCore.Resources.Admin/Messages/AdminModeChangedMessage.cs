namespace RealmCore.Resources.Admin.Messages;

internal record struct AdminModeChangedMessage(Player Player, bool State) : IMessage;
