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
        _marker.CollisionDetection.Entered += HandleEntered;
    }

    private void HandleEntered(RealmMarker marker, Element enteredElement)
    {
        if (Player == enteredElement)
            Complete(this);
    }

    public override void Dispose()
    {
        if (_marker != null)
        {
            _marker.CollisionDetection.Entered -= HandleEntered;
            _marker?.Destroy();
        }
        base.Dispose();
    }
}
