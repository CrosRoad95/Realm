namespace RealmCore.Resources.Admin.Messages;

internal record struct ClearElementsForPlayerMessage(Player player) : IMessage;