using SlipeServer.Server.Elements.Events;

namespace Realm.Domain.Elements.Variants;

public class RPGVariantBlip
{
    private readonly RPGBlip _rpgBlip;
    private HashSet<RPGPlayer> _createdFor = new();

    public RPGVariantBlip(RPGBlip rpgBlip)
    {
        _rpgBlip = rpgBlip;
    }

    public void DestroyFor(RPGPlayer rpgPlayer)
    {
        if (!_createdFor.Contains(rpgPlayer))
            throw new Exception("Blip is already cleared for this player.");

        _rpgBlip.DestroyFor(rpgPlayer);
        _createdFor.Remove(rpgPlayer);
        rpgPlayer.Disconnected -= HandlePlayerDisconnected;
    }

    public void CreateFor(RPGPlayer rpgPlayer, int icon)
    {
        if (!Enum.IsDefined(typeof(BlipIcon), icon))
            throw new Exception("Invalid icon.");

        if (_createdFor.Contains(rpgPlayer))
            DestroyFor(rpgPlayer);

        _rpgBlip.Icon = (BlipIcon)icon;
        _rpgBlip.CreateFor(rpgPlayer);
        _createdFor.Add(rpgPlayer);
        rpgPlayer.Disconnected += HandlePlayerDisconnected;
    }

    private void HandlePlayerDisconnected(Player sender, PlayerQuitEventArgs e)
    {
        DestroyFor((RPGPlayer)sender);
    }
}
