namespace Realm.Server.Elements.Variants;

public class RPGVariantPickup
{
    private readonly RPGPickup _rpgPickup;
    private HashSet<RPGPlayer> _createdFor = new();

    public RPGVariantPickup(RPGPickup rpgPickup)
    {
        _rpgPickup = rpgPickup;
    }

    public void DestroyFor(RPGPlayer player)
    {
        if (!_createdFor.Contains(player))
            throw new Exception("Pickup is already cleared for this player.");

        _rpgPickup.DestroyFor(player);
        _createdFor.Remove(player);
        player.Disconnected -= Player_Disconnected;
    }

    public void CreateFor(RPGPlayer player, ushort model)
    {
        if (!Enum.IsDefined(typeof(PickupModel), model))
            throw new Exception("Invalid pickup model.");

        if (_createdFor.Contains(player))
            DestroyFor(player);

        _rpgPickup.Model = model;
        _rpgPickup.CreateFor(player);
        _createdFor.Add(player);
        player.Disconnected += Player_Disconnected;
    }

    private void Player_Disconnected(Player sender, PlayerQuitEventArgs e)
    {
        DestroyFor((RPGPlayer)sender);
    }
}
