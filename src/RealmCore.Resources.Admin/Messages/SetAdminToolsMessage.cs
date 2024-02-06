namespace RealmCore.Resources.Admin.Messages;

internal record struct SetAdminToolsMessage(Player Player, IReadOnlyList<AdminTool> AdminTools) : IMessage;
