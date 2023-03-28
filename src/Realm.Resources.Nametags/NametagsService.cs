using SlipeServer.Server.Elements;

namespace Realm.Resources.Nametags;

internal sealed class NametagsService : INametagsService
{
    public Action<Ped, string>? HandleSetNametag { get; set; }
    public Action<Ped>? HandleRemoveNametag { get; set; }
    public Action<Player, bool>? HandleSetNametagRenderingEnabled { get; set; }
    public Action<Player, bool>? HandleSetLocalPlayerRenderingEnabled { get; set; }

    public NametagsService()
    {
    }

    public void SetNametagRenderingEnabled(Player player, bool enabled) => HandleSetNametagRenderingEnabled?.Invoke(player, enabled);
    public void SetLocalPlayerRenderingEnabled(Player player, bool enabled) => HandleSetLocalPlayerRenderingEnabled?.Invoke(player, enabled);
    public void RemoveNametag(Ped ped) => HandleRemoveNametag?.Invoke(ped);
    public void SetNametag(Ped ped, string text) => HandleSetNametag?.Invoke(ped, text);

}
