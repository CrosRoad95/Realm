namespace Realm.Server.Elements.Variants;

public class RPGVariantBlip
{
    private readonly RPGBlip _rpgBlip;
    private HashSet<RPGPlayer> _createdFor = new();

    [NoScriptAccess]
    public RPGVariantBlip(RPGBlip rpgBlip)
    {
        _rpgBlip = rpgBlip;
    }

    public void DestroyFor(RPGPlayer player)
    {
        if (!_createdFor.Contains(player))
            throw new Exception("Blip is already cleared for this player.");

        _rpgBlip.DestroyFor(player);
        _createdFor.Remove(player);
        player.Disconnected -= Player_Disconnected;
    }

    public void CreateFor(RPGPlayer player, int icon)
    {
        if (!Enum.IsDefined(typeof(BlipIcon), icon))
            throw new Exception("Invalid icon.");

        if (_createdFor.Contains(player))
            DestroyFor(player);

        _rpgBlip.Icon = (BlipIcon)icon;
        //_rpgBlip.CreateFor(player);
        _rpgBlip.Icon = BlipIcon.Marker;
        _createdFor.Add(player);
        //player.Disconnected += Player_Disconnected;
    }

    private void Player_Disconnected(Player sender, PlayerQuitEventArgs e)
    {
        DestroyFor((RPGPlayer)sender);
    }
}
