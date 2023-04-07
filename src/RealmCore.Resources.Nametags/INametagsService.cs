using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Nametags;

public interface INametagsService
{
    internal Action<Ped, string>? HandleSetNametag { get; set; }
    internal Action<Ped>? HandleRemoveNametag { get; set; }
    internal Action<Player, bool>? HandleSetNametagRenderingEnabled { get; set; }
    internal Action<Player, bool>? HandleSetLocalPlayerRenderingEnabled { get; set; }

    void RemoveNametag(Ped ped);
    void SetNametag(Ped ped, string text);
    void SetNametagRenderingEnabled(Player player, bool enabled);
    void SetLocalPlayerRenderingEnabled(Player player, bool enabled);
}
