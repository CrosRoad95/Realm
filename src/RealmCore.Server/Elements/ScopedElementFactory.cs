namespace RealmCore.Server.Elements;

internal sealed class InnerScopedElementFactory : ScopedElementFactory
{
    public InnerScopedElementFactory(PlayerContext playerContext, ScopedMapIdGenerator elementIdGenerator) : base(playerContext, elementIdGenerator) { }

    public override IScopedElementFactory CreateScope() => throw new NotSupportedException();
}

internal class ScopedElementFactory : IScopedElementFactory
{
    private readonly RealmPlayer _player;
    private bool _disposed;
    public event Action<ScopedElementFactory>? Disposed;
    public event Action<Element>? ElementCreated;
    private readonly object _lock = new();
    private readonly List<Element> _createdElements = [];
    private readonly List<ICollisionDetection> _collisionDetection = [];
    private readonly ScopedMapIdGenerator _elementIdGenerator;

    public IEnumerable<Element> CreatedElements
    {
        get
        {
            lock (_lock)
            {
                foreach (var element in _createdElements)
                {
                    yield return element;
                }
            }
        }
    }

    public IEnumerable<ICollisionDetection> CreatedCollisionDetectionElements
    {
        get
        {
            lock (_lock)
            {
                foreach (var collisionShape in _collisionDetection)
                {
                    yield return collisionShape;
                }
            }
        }
    }

    public ScopedElementFactory(PlayerContext playerContext, ScopedMapIdGenerator elementIdGenerator)
    {
        _player = playerContext.Player;
        _player.Destroyed += HandlePlayerDestroyed;
        _elementIdGenerator = elementIdGenerator;
    }

    private void HandleInnerElementCreated(Element createdElement)
    {
        Add(createdElement);
    }

    public virtual IScopedElementFactory CreateScope()
    {
        var innerScope = new ScopedElementFactory(_player.GetRequiredService<PlayerContext>(), _elementIdGenerator);
        innerScope.ElementCreated += HandleInnerElementCreated;

        void handleDisposed(ScopedElementFactory scopedElementFactory)
        {
            innerScope.ElementCreated -= HandleInnerElementCreated;
        }
        innerScope.Disposed += handleDisposed;
        return innerScope;
    }

    private void HandlePlayerDestroyed(Element _)
    {
        _player.Destroyed -= HandlePlayerDestroyed;
        Dispose();
    }

    private void Add(Element element)
    {
        if(element.Id.Value == 0)
            element.Id = (ElementId)_elementIdGenerator.GetId();

        lock (_lock)
        {
            _createdElements.Add(element);
            if (element is ICollisionDetection collisionDetection)
                _collisionDetection.Add(collisionDetection);
            ElementCreated?.Invoke(element);
            element.Destroyed += HandleDestroyed;
        }
    }

    private void HandleDestroyed(Element element)
    {
        lock (_lock)
        {
            element.Destroyed -= HandleDestroyed;
            _createdElements.Remove(element);
            if (element is ICollisionDetection collisionDetection)
                _collisionDetection.Remove(collisionDetection);
        }
    }

    private void AssociateWithPlayer(Element element)
    {
        Add(element);
        element.AssociateWith(_player);
        if (element is RealmMarker pickup)
        {
            if(pickup.CollisionShape.Id.Value == 0)
                pickup.CollisionShape.Id = (ElementId)_elementIdGenerator.GetId();
            pickup.CollisionShape.AssociateWith(_player);
        }

        if (element is RealmMarker marker)
        {
            if (marker.CollisionShape.Id.Value == 0)
                marker.CollisionShape.Id = (ElementId)_elementIdGenerator.GetId();
            marker.CollisionShape.AssociateWith(_player);
        }
    }

    public RealmCollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte? interior = null, ushort? dimension = null, Action<RealmCollisionSphere>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var collisionSphere = new RealmCollisionSphere(_player.ServiceProvider, position, radius)
        {
            Interior = interior ?? _player.Interior,
            Dimension = dimension ?? _player.Dimension
        };

        elementBuilder?.Invoke(collisionSphere);
        AssociateWithPlayer(collisionSphere);
        return collisionSphere;
    }

    public RealmMarker CreateMarker(Vector3 position, MarkerType markerType, float size, Color color, byte? interior = null, ushort? dimension = null, Action<RealmMarker>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var marker = new RealmMarker(_player.ServiceProvider, position, markerType, size)
        {
            Color = color,
            Interior = interior ?? _player.Interior,
            Dimension = dimension ?? _player.Dimension
        };

        elementBuilder?.Invoke(marker);
        AssociateWithPlayer(marker);
        return marker;
    }

    public RealmBlip CreateBlip(Vector3 position, BlipIcon blipIcon, byte? interior = null, ushort? dimension = null, Action<RealmBlip>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var blip = new RealmBlip(position, blipIcon)
        {
            Interior = interior ?? _player.Interior,
            Dimension = dimension ?? _player.Dimension
        };

        elementBuilder?.Invoke(blip);
        AssociateWithPlayer(blip);
        return blip;
    }

    public RealmCollisionCircle CreateCollisionCircle(Vector2 position, float radius, byte? interior = null, ushort? dimension = null, Action<RealmCollisionCircle>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmCollisionCuboid CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte? interior = null, ushort? dimension = null, Action<RealmCollisionCuboid>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmCollisionPolygon CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte? interior = null, ushort? dimension = null, Action<RealmCollisionPolygon>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmCollisionRectangle CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte? interior = null, ushort? dimension = null, Action<RealmCollisionRectangle>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmCollisionTube CreateCollisionTube(Vector3 position, float radius, float height, byte? interior = null, ushort? dimension = null, Action<RealmCollisionTube>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmMarker CreateMarker(Vector3 position, MarkerType markerType, Color color, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmPed CreatePed(PedModel pedModel, Vector3 position, byte? interior = null, ushort? dimension = null, Action<RealmPed>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmPickup CreatePickup(Vector3 position, ushort model, byte? interior = null, ushort? dimension = null, Action<RealmPickup>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmRadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte? interior = null, ushort? dimension = null, Action<RealmRadarArea>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmVehicle CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Action<RealmVehicle>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmWorldObject CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Action<RealmWorldObject>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var worldObject = new RealmWorldObject(model, position)
        {
            Rotation = rotation,
            Interior = interior ?? _player.Interior,
            Dimension = dimension ?? _player.Dimension
        };

        elementBuilder?.Invoke(worldObject);
        AssociateWithPlayer(worldObject);
        return worldObject;
    }

    public FocusableRealmWorldObject CreateFocusableObject(ObjectModel model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Action<RealmWorldObject>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var worldObject = new FocusableRealmWorldObject(model, position)
        {
            Rotation = rotation,
            Interior = interior ?? _player.Interior,
            Dimension = dimension ?? _player.Dimension
        };

        elementBuilder?.Invoke(worldObject);
        AssociateWithPlayer(worldObject);
        return worldObject;
    }

    RealmPickup IElementFactory.CreatePickup(Vector3 position, ushort model, byte? interior = null, ushort? dimension = null, Action<RealmPickup>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    RealmVehicle IElementFactory.CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Action<RealmVehicle>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public void RelayCreated(Element element)
    {
        ElementCreated?.Invoke(element);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    public void Dispose()
    {
        List<Element> elementsToRemove;
        lock (_lock)
            elementsToRemove = new(_createdElements);

        foreach (var element in elementsToRemove)
        {
            element.Destroy();
        }
        Disposed?.Invoke(this);
        _disposed = true;
    }
}
