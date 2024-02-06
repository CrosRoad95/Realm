namespace RealmCore.Resources.Admin.Messages;

internal record struct UpdateElementsComponentsMessage(Player Player, LuaValue elementsComponents) : IMessage;
