namespace RealmCore.Server.Concepts.Objectives;

public class TransportObjectObjective : Objective
{
    private readonly Element? _element;
    private readonly Vector3 _position;
    private readonly bool _createMarker;
    private RealmMarker? _marker;
    private IElementCollection _elementCollection = default!;
    public Func<Element, bool>? CheckElement { get; set; }

    public override Vector3 Position => _position;

    public TransportObjectObjective(Element element, Vector3 position, bool createMarker = true)
    {
        _element = element;
        _position = position;
        _createMarker = createMarker;

        _element.Destroyed += HandleDestroyed;
    }

    private void HandleDestroyed(Element element)
    {
        if(_element != null)
        {
            _element.Destroyed -= HandleDestroyed;
            Incomplete(this);
        }
    }

    public TransportObjectObjective(Vector3 position, bool createMarker = true)
    {
        _position = position;
        _createMarker = createMarker;
    }

    protected override void Load(RealmPlayer player)
    {
        var elementFactory = player.ServiceProvider.GetRequiredService<IElementFactory>();
        _elementCollection = player.ServiceProvider.GetRequiredService<IElementCollection>();
        // TODO: Create marker
    }

    private void HandleElementEntered(Element element)
    {
        if (element is not IComponents components)
            return;

        if (components.TryGetComponent(out OwnerComponent ownerComponent))
        {
            if (ownerComponent.OwningElement == element)
            {
                if (CheckElement != null)
                {
                    if (CheckElement(element))
                    {
                        Complete(this, element);
                        return;
                    }
                }
                Complete(this, element);
            }
        }
    }

    public override void Update()
    {
        if (_marker == null)
            return;

        if (_element == null)
        {
            var elements = _elementCollection.GetWithinRange(_marker.Position, _marker.CollisionShape.Radius);
            foreach (var element in elements)
                _marker.CollisionShape.CheckElementWithin(element);
        }
        else
        {
            if(_element is IComponents components && components.TryGetComponent(out LiftableWorldObjectComponent liftableWorldObjectComponent))
            {
                if (liftableWorldObjectComponent.Owner == null) // Accept only dropped elements.
                    _marker.CollisionShape.CheckElementWithin(_element);
            }
            else
            {
                _marker.CollisionShape.CheckElementWithin(_element);
            }
        }
    }

    public override void Dispose()
    {
        _marker?.Destroy();
        CheckElement = null;
        base.Dispose();
    }
}
