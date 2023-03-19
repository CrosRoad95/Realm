using SlipeServer.Server.Elements;

namespace Realm.Resources.Nametags;

public class NametagsService
{
    internal Action<Ped, string>? HandleSetNametag;
    internal Action<Ped>? HandleRemoveNametag;
    internal Action<Player, bool>? HandleSetNametagRenderingEnabled;

    public NametagsService()
    {
    }

    public void SetNametagRenderingEnabled(Player player, bool enabled) => HandleSetNametagRenderingEnabled?.Invoke(player, enabled);
    public void RemoveNametag(Ped ped) => HandleRemoveNametag?.Invoke(ped);
    public void SetNametag(Ped ped, string text) => HandleSetNametag?.Invoke(ped, text);

}
