namespace RealmCore.Server.Concepts.Objectives;

public class MarkerEnterObjective : Objective
{
    private readonly Vector3 _position;

    private RealmMarker? _marker;

    public override Vector3 Position => _position;

    public MarkerEnterObjective(Vector3 position)
    {
        _position = position;
    }

    protected override void Load()
    {
        if(_marker != null)
        {
            ;
        }
        _marker = ElementFactory.CreateMarker(_position, MarkerType.Arrow, Color.White);
        _marker.CollisionDetection.Entered += HandleEntered;
    }

    private void HandleEntered(RealmMarker marker, Element enteredElement)
    {
        if (Player == enteredElement)
            Complete(this);
    }

    public override void Dispose()
    {
        if(_marker != null)
        {
            _marker.CollisionDetection.Entered -= HandleEntered;
            _marker?.Destroy();
        }
        base.Dispose();
    }
}
