using RealmCore.Resources.Admin.Enums;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Admin;

public interface IAdminService
{
    internal Action<IMessage>? MessageHandler { get; set; }

    void SetAdminModeEnabledForPlayer(Player player, bool enabled);
    void SetAdminTools(Player player, IReadOnlyList<AdminTool> adminTools);
}
