using SlipeServer.Server.Elements.Events;

namespace Realm.Domain.Elements.Variants;

public class RPGVariantRadarArea
{
    private readonly RPGRadarArea _rpgRadarArea;
    private HashSet<RPGPlayer> _createdFor = new();

    public RPGVariantRadarArea(RPGRadarArea rpgRadarArea)
    {
        _rpgRadarArea = rpgRadarArea;
    }

    public void DestroyFor(RPGPlayer rpgPlayer)
    {
        if (!_createdFor.Contains(rpgPlayer))
            throw new Exception("RadarArea is already cleared for this player.");

        _rpgRadarArea.DestroyFor(rpgPlayer);
        _createdFor.Remove(rpgPlayer);
        rpgPlayer.Disconnected -= HandlePlayerDisconnected;
    }

    public void CreateFor(RPGPlayer rpgPlayer, Color color, bool flashing = false)
    {
        if (_createdFor.Contains(rpgPlayer))
            DestroyFor(rpgPlayer);

        _rpgRadarArea.Color = color;
        _rpgRadarArea.IsFlashing = flashing;
        _rpgRadarArea.CreateFor(rpgPlayer);
        _createdFor.Add(rpgPlayer);
        rpgPlayer.Disconnected += HandlePlayerDisconnected;
    }

    private void HandlePlayerDisconnected(Player sender, PlayerQuitEventArgs e)
    {
        DestroyFor((RPGPlayer)sender);
    }
}
