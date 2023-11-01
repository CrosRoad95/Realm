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
        return (T)LastCreatedElement ?? throw new InvalidOperationException();
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

    public RealmCollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte interior = 0, ushort dimension = 0, Func<RealmCollisionSphere, IEnumerable<IComponent>>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var collisionSphere = new RealmCollisionSphere(_player.ServiceProvider, position, radius)
        {
            Interior = interior,
            Dimension = dimension
        };

        ExecuteElementBuilder(elementBuilder, collisionSphere);
        AssociateWithPlayer(collisionSphere);
        return collisionSphere;
    }

    public RealmMarker CreateMarker(MarkerType markerType, Vector3 position, Color color, byte interior = 0, ushort dimension = 0, Func<RealmMarker, IEnumerable<IComponent>>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var marker = new RealmMarker(_player.ServiceProvider, position, markerType, 2)
        {
            Color = color,
            Interior = interior,
            Dimension = dimension
        };

        ExecuteElementBuilder(elementBuilder, marker);
        AssociateWithPlayer(marker);
        return marker;
    }

    public RealmObject CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Func<RealmObject, IEnumerable<IComponent>>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var worldObject = new RealmObject(_player.ServiceProvider, model, position)
        {
            Rotation = rotation,
            Interior = interior,
            Dimension = dimension
        };

        ExecuteElementBuilder(elementBuilder, worldObject);
        AssociateWithPlayer(worldObject);
        return worldObject;
    }

    public RealmBlip CreateBlip(Vector3 position, BlipIcon blipIcon, byte interior = 0, ushort dimension = 0, Func<RealmBlip, IEnumerable<IComponent>>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var blip = new RealmBlip(_player.ServiceProvider, position, blipIcon)
        {
            Interior = interior,
            Dimension = dimension
        };

        ExecuteElementBuilder(elementBuilder, blip);
        AssociateWithPlayer(blip);
        return blip;
    }

    public CollisionCircle CreateCollisionCircle(Vector2 position, float radius, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public CollisionCuboid CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public CollisionPolygon CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public CollisionRectangle CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public CollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public CollisionTube CreateCollisionTube(Vector3 position, float radius, float height, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmMarker CreateMarker(Vector3 position, MarkerType markerType, Color color, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public Ped CreatePed(PedModel pedModel, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmPickup CreatePickup(Vector3 position, ushort model, byte interior = 0, ushort dimension = 0, Func<RealmPickup, IEnumerable<IComponent>>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmVehicle CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Func<RealmVehicle, IEnumerable<IComponent>>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    T IScopedElementFactory.GetLastCreatedElement<T>()
    {
        throw new NotImplementedException();
    }

    RealmBlip IElementFactory.CreateBlip(Vector3 position, BlipIcon blipIcon, byte interior, ushort dimension, Func<RealmBlip, IEnumerable<IComponent>>? elementBuilder)
    {
        throw new NotImplementedException();
    }

    CollisionCircle IElementFactory.CreateCollisionCircle(Vector2 position, float radius, byte interior, ushort dimension, Action<Element>? elementBuilder)
    {
        throw new NotImplementedException();
    }

    CollisionCuboid IElementFactory.CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte interior, ushort dimension, Action<Element>? elementBuilder)
    {
        throw new NotImplementedException();
    }

    CollisionPolygon IElementFactory.CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte interior, ushort dimension, Action<Element>? elementBuilder)
    {
        throw new NotImplementedException();
    }

    CollisionRectangle IElementFactory.CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte interior, ushort dimension, Action<Element>? elementBuilder)
    {
        throw new NotImplementedException();
    }

    RealmCollisionSphere IElementFactory.CreateCollisionSphere(Vector3 position, float radius, byte interior, ushort dimension, Func<RealmCollisionSphere, IEnumerable<IComponent>>? elementBuilder)
    {
        throw new NotImplementedException();
    }

    CollisionTube IElementFactory.CreateCollisionTube(Vector3 position, float radius, float height, byte interior, ushort dimension, Action<Element>? elementBuilder)
    {
        throw new NotImplementedException();
    }

    RealmMarker IElementFactory.CreateMarker(Vector3 position, MarkerType markerType, Color color, byte interior, ushort dimension, Action<Element>? elementBuilder)
    {
        throw new NotImplementedException();
    }

    RealmObject IElementFactory.CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte interior, ushort dimension, Func<RealmObject, IEnumerable<IComponent>>? elementBuilder)
    {
        throw new NotImplementedException();
    }

    Ped IElementFactory.CreatePed(PedModel pedModel, Vector3 position, byte interior, ushort dimension, Action<Element>? elementBuilder)
    {
        throw new NotImplementedException();
    }

    RealmPickup IElementFactory.CreatePickup(Vector3 position, ushort model, byte interior, ushort dimension, Func<RealmPickup, IEnumerable<IComponent>>? elementBuilder)
    {
        throw new NotImplementedException();
    }

    RadarArea IElementFactory.CreateRadarArea(Vector2 position, Vector2 size, Color color, byte interior, ushort dimension, Action<Element>? elementBuilder)
    {
        throw new NotImplementedException();
    }

    RealmVehicle IElementFactory.CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior, ushort dimension, Func<RealmVehicle, IEnumerable<IComponent>>? elementBuilder)
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

    public void Dispose()
    {
        Disposed?.Invoke(this);
        _disposed = true;
    }
}
