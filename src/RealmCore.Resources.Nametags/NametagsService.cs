namespace RealmCore.Resources.Nametags;

public interface INametagsService
{
    internal Action<Ped, string>? RelaySetNametag { get; set; }
    internal Action<Ped>? RelayRemoveNametag { get; set; }
    internal Action<Player, bool>? RelaySetNametagRenderingEnabled { get; set; }
    internal Action<Player, bool>? RelaySetLocalPlayerRenderingEnabled { get; set; }

    void RemoveNametag(Ped ped);
    void SetNametag(Ped ped, string text);
    void SetNametagRenderingEnabled(Player player, bool enabled);
    void SetLocalPlayerRenderingEnabled(Player player, bool enabled);
}

internal sealed class NametagsService : INametagsService
{
    public Action<Ped, string>? RelaySetNametag { get; set; }
    public Action<Ped>? RelayRemoveNametag { get; set; }
    public Action<Player, bool>? RelaySetNametagRenderingEnabled { get; set; }
    public Action<Player, bool>? RelaySetLocalPlayerRenderingEnabled { get; set; }

    public NametagsService()
    {
    }

    public void SetNametagRenderingEnabled(Player player, bool enabled) => RelaySetNametagRenderingEnabled?.Invoke(player, enabled);
    public void SetLocalPlayerRenderingEnabled(Player player, bool enabled) => RelaySetLocalPlayerRenderingEnabled?.Invoke(player, enabled);
    public void RemoveNametag(Ped ped) => RelayRemoveNametag?.Invoke(ped);
    public void SetNametag(Ped ped, string text) => RelaySetNametag?.Invoke(ped, text);
}
