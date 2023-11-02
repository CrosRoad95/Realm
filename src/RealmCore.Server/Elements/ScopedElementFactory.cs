namespace RealmCore.Server.Elements;

internal sealed class ScopedElementFactory : IScopedElementFactory
{
    private readonly RealmPlayer _player;
    private Element? _lastCreatedElement;
    private bool _disposed;
    public event Action<ScopedElementFactory>? Disposed;
    public event Action<Element>? ElementCreated;
    private readonly object _lock = new();
    private readonly List<Element> _createdElements = new();
    private readonly ScopedMapIdGenerator _elementIdGenerator;

    public Element? LastCreatedElement => _lastCreatedElement ?? throw new NullReferenceException();
    public IEnumerable<Element> CreatedElements
    {
        get
        {
            lock (_lock)
            {
                foreach (var item in _createdElements)
                {
                    yield return item;
                }
            }
        }
    }

    public T GetLastCreatedElement<T>() where T : Element
    {
        return (T?)LastCreatedElement ?? throw new InvalidOperationException();
    }

    private void ExecuteElementBuilder<TElement>(Func<TElement, IEnumerable<IComponent>>? builder, TElement element) where TElement : IComponents
    {
        if (builder == null)
            return;

        var components = builder(element);
        foreach (var component in components)
        {
            element.AddComponent(component);
        }
    }

    public ScopedElementFactory(PlayerContext playerContext, ScopedMapIdGenerator elementIdGenerator)
    {
        _player = playerContext.Player;
        _player.Destroyed += HandlePlayerDestroyed;
        _elementIdGenerator = elementIdGenerator;
    }

    event Action<Element>? IElementFactory.ElementCreated
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    private void HandlePlayerDestroyed(Element _)
    {
        _player.Destroyed -= HandlePlayerDestroyed;
        Dispose();
    }

    private void Add(Element element)
    {
        element.Id = (ElementId)_elementIdGenerator.GetId();
        lock (_lock)
        {
            _lastCreatedElement = element;
            _createdElements.Add(element);
            ElementCreated?.Invoke(element);
        }
    }

    private void AssociateWithPlayer(Element element)
    {
        Add(element);
        element.AssociateWith(_player);
        if (element is RealmMarker pickup)
        {
            pickup.CollisionShape.AssociateWith(_player);
        }

        if (element is RealmMarker marker)
        {
            marker.CollisionShape.AssociateWith(_player);
        }
    }

    public RealmCollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte? interior = null, ushort? dimension = null, Func<RealmCollisionSphere, IEnumerable<IComponent>>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var collisionSphere = new RealmCollisionSphere(_player.ServiceProvider, position, radius)
        {
            Interior = interior ?? _player.Interior,
            Dimension = dimension ?? _player.Dimension
        };

        ExecuteElementBuilder(elementBuilder, collisionSphere);
        AssociateWithPlayer(collisionSphere);
        return collisionSphere;
    }

    public RealmMarker CreateMarker(MarkerType markerType, Vector3 position, Color color, byte? interior = null, ushort? dimension = null, Func<RealmMarker, IEnumerable<IComponent>>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var marker = new RealmMarker(_player.ServiceProvider, position, markerType, 2)
        {
            Color = color,
            Interior = interior ?? _player.Interior,
            Dimension = dimension ?? _player.Dimension
        };

        ExecuteElementBuilder(elementBuilder, marker);
        AssociateWithPlayer(marker);
        return marker;
    }

    public RealmObject CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Func<RealmObject, IEnumerable<IComponent>>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var worldObject = new RealmObject(_player.ServiceProvider, model, position)
        {
            Rotation = rotation,
            Interior = interior ?? _player.Interior,
            Dimension = dimension ?? _player.Dimension
        };

        ExecuteElementBuilder(elementBuilder, worldObject);
        AssociateWithPlayer(worldObject);
        return worldObject;
    }

    public RealmBlip CreateBlip(Vector3 position, BlipIcon blipIcon, byte? interior = null, ushort? dimension = null, Func<RealmBlip, IEnumerable<IComponent>>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var blip = new RealmBlip(_player.ServiceProvider, position, blipIcon)
        {
            Interior = interior ?? _player.Interior,
            Dimension = dimension ?? _player.Dimension
        };

        ExecuteElementBuilder(elementBuilder, blip);
        AssociateWithPlayer(blip);
        return blip;
    }

    public CollisionCircle CreateCollisionCircle(Vector2 position, float radius, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public CollisionCuboid CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public CollisionPolygon CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public CollisionRectangle CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public CollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public CollisionTube CreateCollisionTube(Vector3 position, float radius, float height, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmMarker CreateMarker(Vector3 position, MarkerType markerType, Color color, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public Ped CreatePed(PedModel pedModel, Vector3 position, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmPickup CreatePickup(Vector3 position, ushort model, byte? interior = null, ushort? dimension = null, Func<RealmPickup, IEnumerable<IComponent>>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmVehicle CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Func<RealmVehicle, IEnumerable<IComponent>>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    RealmBlip IElementFactory.CreateBlip(Vector3 position, BlipIcon blipIcon, byte? interior = null, ushort? dimension = null, Func<RealmBlip, IEnumerable<IComponent>>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    CollisionCircle IElementFactory.CreateCollisionCircle(Vector2 position, float radius, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    CollisionCuboid IElementFactory.CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    CollisionPolygon IElementFactory.CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    CollisionRectangle IElementFactory.CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    RealmCollisionSphere IElementFactory.CreateCollisionSphere(Vector3 position, float radius, byte? interior = null, ushort? dimension = null, Func<RealmCollisionSphere, IEnumerable<IComponent>>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    CollisionTube IElementFactory.CreateCollisionTube(Vector3 position, float radius, float height, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    RealmMarker IElementFactory.CreateMarker(Vector3 position, MarkerType markerType, Color color, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    RealmObject IElementFactory.CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Func<RealmObject, IEnumerable<IComponent>>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    Ped IElementFactory.CreatePed(PedModel pedModel, Vector3 position, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    RealmPickup IElementFactory.CreatePickup(Vector3 position, ushort model, byte? interior = null, ushort? dimension = null, Func<RealmPickup, IEnumerable<IComponent>>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    RadarArea IElementFactory.CreateRadarArea(Vector2 position, Vector2 size, Color color, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    RealmVehicle IElementFactory.CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Func<RealmVehicle, IEnumerable<IComponent>>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    void IElementFactory.RelayCreated(Element element)
    {
        throw new NotSupportedException();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    T IScopedElementFactory.GetLastCreatedElement<T>()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        Disposed?.Invoke(this);
        _disposed = true;
    }
}
