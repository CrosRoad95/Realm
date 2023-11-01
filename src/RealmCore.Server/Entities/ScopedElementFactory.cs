
namespace RealmCore.Server.Entities;

internal sealed class ScopedElementFactory : IScopedElementFactory
{
    private readonly RealmPlayer _player;
    private Element? _lastCreatedElement;
    private bool _disposed;
    public event Action<ScopedElementFactory>? Disposed;
    public event Action<Element>? ElementCreated;
    private readonly object _lock = new();
    private readonly List<Element> _createdElements = new();

    public RealmPlayer Player => _player;
    public Element LastCreatedElement => _lastCreatedElement ?? throw new NullReferenceException();
    public IEnumerable<Element> CreatedElements
    {
        get
        {
            lock(_lock)
            {
                foreach (var item in _createdElements)
                {
                    yield return item;
                }
            }
        }
    }

    public T GetLastCreatedElement<T>() where T: Element
    {
        return (T)LastCreatedElement;
    }

    public ScopedElementFactory(RealmPlayer player)
    {
        _player = player;
        _player.Destroyed += HandlePlayerDestroyed;
    }

    private void HandlePlayerDestroyed(Element _)
    {
        _player.Destroyed -= HandlePlayerDestroyed;
        Dispose();
    }

    private void Add(Element element)
    {
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
        if (element is Pickup pickup)
        {
            pickup.CollisionShape.AssociateWith(_player);
        }

        if (element is Marker marker)
        {
            // TODO:
            //marker.CollisionShape.AssociateWith(_player);
        }
    }

    public RealmCollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte interior = 0, ushort dimension = 0, Action<RealmCollisionSphere>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var collisionSphere = new RealmCollisionSphere(_player.ServiceProvider, position, radius)
        {
            Interior = interior,
            Dimension = dimension
        };

        AssociateWithPlayer(collisionSphere);
        return collisionSphere;
    }

    public RealmMarker CreateMarker(MarkerType markerType, Vector3 position, Color color, byte interior = 0, ushort dimension = 0, Action<Marker>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var marker = new RealmMarker(_player.ServiceProvider, position, markerType, 2)
        {
            Color = color,
            Interior = interior,
            Dimension = dimension
        };

        AssociateWithPlayer(marker);
        return marker;
    }

    public RealmObject CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Action<RealmObject>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var worldObject = new RealmObject(_player.ServiceProvider, model, position)
        {
            Rotation = rotation,
            Interior = interior,
            Dimension = dimension
        };

        AssociateWithPlayer(worldObject);
        return worldObject;
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

    public Blip CreateBlip(Vector3 position, BlipIcon blipIcon, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
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

    public WorldObject CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public Ped CreatePed(PedModel pedModel, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmPickup CreatePickup(Vector3 position, ushort model, byte interior = 0, ushort dimension = 0, Action<RealmPickup>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmVehicle CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Action<RealmVehicle>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }
}
