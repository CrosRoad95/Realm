using RealmCore.Resources.Admin.Enums;
using RealmCore.Resources.Admin.Messages;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Admin;

internal sealed class AdminService : IAdminService
{
    public Action<IMessage>? MessageHandler { get; set; }

    public AdminService()
    {

    }

    public void SetAdminModeEnabledForPlayer(Player player, bool enabled)
    {
        MessageHandler?.Invoke(new AdminModeChangedMessage(player, enabled));
    }

    public void SetAdminTools(Player player, IReadOnlyList<AdminTool> adminTools)
    {
        MessageHandler?.Invoke(new SetAdminToolsMessage(player, adminTools));
    }
}
