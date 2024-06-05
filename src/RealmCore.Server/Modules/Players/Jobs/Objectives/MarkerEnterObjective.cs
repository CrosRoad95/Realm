namespace RealmCore.Server.Modules.Players.Jobs.Objectives;

public class MarkerEnterObjective : Objective
{
    private readonly Location _destination;

    private RealmMarker? _marker;

    public override Location Location => _destination;

    public MarkerEnterObjective(Location destination)
    {
        _destination = destination;
    }

    protected override void Load()
    {
        _marker = ElementFactory.CreateMarker(_destination, MarkerType.Arrow, 1, Color.White);
        _marker.CollisionShape.ElementEntered += HandleElementEntered;
    }

    private void HandleElementEntered(CollisionShape sender, CollisionShapeHitEventArgs e)
    {
        if (Player == e.Element)
            Complete(this);
    }

    public override string ToString() => "Wejdź w marker";

    public override void Dispose()
    {
        if (_marker != null)
        {
            _marker.CollisionShape.ElementEntered -= HandleElementEntered;
            _marker?.Destroy();
        }
        base.Dispose();
    }
}
