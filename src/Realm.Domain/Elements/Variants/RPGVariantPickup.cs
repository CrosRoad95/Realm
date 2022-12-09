using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;

namespace Realm.Domain.Elements.Variants;

public class RPGVariantPickup
{
    private readonly RPGPickup _rpgPickup;
    private HashSet<RPGPlayer> _createdFor = new();

    public RPGVariantPickup(RPGPickup rpgPickup)
    {
        _rpgPickup = rpgPickup;
    }

    public void DestroyFor(RPGPlayer rpgPlayer)
    {
        if (!_createdFor.Contains(rpgPlayer))
            throw new Exception("Pickup is already cleared for this player.");

        _rpgPickup.DestroyFor(rpgPlayer);
        _createdFor.Remove(rpgPlayer);
        rpgPlayer.Disconnected -= HandlePlayerDisconnected;
    }

    public void CreateFor(RPGPlayer rpgPlayer, ushort model)
    {
        if (!Enum.IsDefined(typeof(PickupModel), model))
            throw new Exception("Invalid pickup model.");

        if (_createdFor.Contains(rpgPlayer))
            DestroyFor(rpgPlayer);

        _rpgPickup.Model = model;
        _rpgPickup.CreateFor(rpgPlayer);
        _createdFor.Add(rpgPlayer);
        rpgPlayer.Disconnected += HandlePlayerDisconnected;
    }

    private void HandlePlayerDisconnected(Player sender, PlayerQuitEventArgs e)
    {
        DestroyFor((RPGPlayer)sender);
    }
}
