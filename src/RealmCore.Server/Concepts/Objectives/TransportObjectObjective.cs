namespace RealmCore.Server.Concepts.Objectives;

public class TransportObjectObjective : Objective
{
    private Element? _element;
    private readonly Vector3 _destination;
    private readonly float _range;
    private readonly bool _withMarker;
    private RealmMarker? _marker;
    private bool _loaded = false;
    private IElementCollection _elementCollection = default!;
    private IScopedElementFactory _scopedElementFactory = default!;
    public Func<Element, bool>? CheckElement { get; set; }

    public override Vector3 Position => _destination;

    public TransportObjectObjective(Element element, Vector3 destination, float range = 2)
    {
        _element = element;
        _destination = destination;
        _range = range;

        _element.Destroyed += HandleDestroyed;
    }

    public TransportObjectObjective(Vector3 destination, float range = 2, bool withMarker = false)
    {
        _destination = destination;
        _range = range;
        _withMarker = withMarker;
    }

    private void HandleDestroyed(Element element)
    {
        if(_element != null)
        {
            _element.Destroyed -= HandleDestroyed;
            Incomplete(this);
        }
    }

    protected override void Load()
    {
        _elementCollection = Player.GetRequiredService<IElementCollection>();
        _scopedElementFactory = Player.GetRequiredService<IScopedElementFactory>();
        if(_withMarker)
            _marker = ElementFactory.CreateMarker(_destination, MarkerType.Cylinder, 1, Color.Red);
        _loaded = true;
    }

    public override void Update()
    {
        if (!_loaded)
            return;

        if (_element == null)
        {
            var elementsInRange = _elementCollection.GetWithinRange(_destination, _range);
            foreach (var element in elementsInRange)
            {
                if (element.TryGetComponent(out OwnerComponent ownerComponent))
                {
                    if (ownerComponent.OwningElement == Player)
                    {
                        if (element.TryGetComponent(out LiftableWorldObjectComponent liftableWorldObjectComponent))
                        {
                            if (liftableWorldObjectComponent.Owner == null)
                                Complete(this, element);
                        }
                        else
                            Complete(this, element);
                    }
                }
            }
            foreach (var element in _scopedElementFactory.CreatedElements.ToList())
            {
                if (element.TryGetComponent(out OwnerComponent ownerComponent))
                {
                    if (ownerComponent.OwningElement == Player)
                    {
                        if (element.TryGetComponent(out LiftableWorldObjectComponent liftableWorldObjectComponent))
                        {
                            if (liftableWorldObjectComponent.Owner == null)
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
            if(Vector3.DistanceSquared(_element.Position, _destination) <= (_range * _range))
            {
                if(_element.TryGetComponent(out LiftableWorldObjectComponent liftableWorldObjectComponent))
                    if (liftableWorldObjectComponent.Owner == null)
                        Complete(this, _element);
            }
        }
    }

    public override void Dispose()
    {
        if (_element != null)
        {
            _element.Destroy();
            _element = null;
        }
        
        if (_marker != null)
        {
            _marker.Destroy();
            _marker = null;
        }

        CheckElement = null;
        base.Dispose();
    }
}
