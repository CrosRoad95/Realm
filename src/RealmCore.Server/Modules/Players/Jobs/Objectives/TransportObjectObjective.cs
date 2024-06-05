namespace RealmCore.Server.Modules.Players.Jobs.Objectives;

public class TransportObjectObjective : Objective
{
    private Element? _element;
    private readonly Location _destination;
    private readonly float _range;
    private IElementCollection _elementCollection = default!;
    private IScopedElementFactory _scopedElementFactory = default!;
    public Func<Element, bool>? CheckElement { get; set; }

    public override Location Location => _destination;

    public TransportObjectObjective(Element element, Location destination, float range = 2)
    {
        _element = element;
        _destination = destination;
        _range = range;

        _element.Destroyed += HandleDestroyed;
    }

    public TransportObjectObjective(Location destination, float range = 2)
    {
        _destination = destination;
        _range = range;
    }

    private void HandleDestroyed(Element element)
    {
        if (_element != null)
        {
            _element.Destroyed -= HandleDestroyed;
            Incomplete(this);
        }
    }

    protected override void Load()
    {
        _elementCollection = Player.GetRequiredService<IElementCollection>();
        _scopedElementFactory = Player.GetRequiredService<IScopedElementFactory>();
    }

    public override void Update()
    {
        if (_element == null)
        {
            // TODO: Check interior, dimension
            var elementsInRange = _elementCollection.GetWithinRange(_destination.Position, _range);
            foreach (var element in elementsInRange)
            {
                if (element is RealmWorldObject worldObject)
                {
                    if (worldObject.Owner == Player)
                    {
                        if (worldObject.Interaction is LiftableInteraction liftableInteraction)
                        {
                            if (liftableInteraction.Owner == null || liftableInteraction.Owner == Player)
                                Complete(this, element);
                        }
                        else
                            Complete(this, element);
                    }
                }
            }
            foreach (var element in _scopedElementFactory.CreatedElements.ToArray())
            {
                if (element is RealmWorldObject worldObject)
                {
                    if (worldObject.Owner == Player)
                    {
                        if (worldObject.Interaction is LiftableInteraction liftableInteraction)
                        {
                            if (liftableInteraction.Owner == null || liftableInteraction.Owner == Player)
                                Complete(this, element);
                        }
                        else
                            Complete(this, element);
                    }
                }
            }
        }
        else
        {
            // TODO: Check interior, dimension
            if (Vector3.DistanceSquared(_element.Position, _destination.Position) <= _range * _range)
            {
                if (_element is RealmWorldObject worldObject)
                {
                    if (worldObject.Interaction is LiftableInteraction liftableInteraction)
                        if (liftableInteraction.Owner == null || liftableInteraction.Owner == Player)
                            Complete(this, _element);
                }
            }
        }
    }

    public override string ToString() => "Przetransportuj obiekt";

    public override void Dispose()
    {
        if (_element != null)
        {
            _element.Destroy();
            _element = null;
        }

        CheckElement = null;
        base.Dispose();
    }
}
