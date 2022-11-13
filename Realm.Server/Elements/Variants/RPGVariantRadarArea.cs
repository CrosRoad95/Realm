namespace Realm.Server.Elements.Variants;

public class RPGVariantRadarArea
{
    private readonly RPGRadarArea _rpgRadarArea;
    private HashSet<RPGPlayer> _createdFor = new();

    public RPGVariantRadarArea(RPGRadarArea rpgRadarArea)
    {
        _rpgRadarArea = rpgRadarArea;
    }

    public void DestroyFor(RPGPlayer player)
    {
        if (!_createdFor.Contains(player))
            throw new Exception("RadarArea is already cleared for this player.");

        _rpgRadarArea.DestroyFor(player);
        _createdFor.Remove(player);
        player.Disconnected -= Player_Disconnected;
    }

    public void CreateFor(RPGPlayer player, Color color, bool flashing = false)
    {
        if (_createdFor.Contains(player))
            DestroyFor(player);

        _rpgRadarArea.Color = color;
        _rpgRadarArea.IsFlashing = flashing;
        _rpgRadarArea.CreateFor(player);
        _createdFor.Add(player);
        player.Disconnected += Player_Disconnected;
    }

    private void Player_Disconnected(Player sender, PlayerQuitEventArgs e)
    {
        DestroyFor((RPGPlayer)sender);
    }
}
