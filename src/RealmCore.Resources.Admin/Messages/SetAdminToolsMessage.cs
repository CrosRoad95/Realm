using RealmCore.Resources.Admin.Enums;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Admin.Messages;

internal record struct SetAdminToolsMessage(Player Player, IReadOnlyList<AdminTool> AdminTools) : IMessage;
