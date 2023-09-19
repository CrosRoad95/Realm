namespace RealmCore.Server.Entities;

internal sealed class ScopedEntityFactory : IScopedEntityFactory
{
    private readonly Entity _entity;
    private readonly IElementCollection _elementCollection;
    private readonly IEntityEngine _entityEngine;
    private PlayerPrivateElementComponentBase? _lastCreatedComponent;
    private bool _disposed;
    public event Action<ScopedEntityFactory>? Disposed;
    private readonly object _lock = new();
    private readonly List<PlayerPrivateElementComponentBase> _createdComponents = new();

    public Entity Entity => _entity;
    public PlayerPrivateElementComponentBase LastCreatedComponent => _lastCreatedComponent ?? throw new NullReferenceException();
    public IEnumerable<PlayerPrivateElementComponentBase> CreatedComponents
    {
        get
        {
            lock(_lock)
            {
                foreach (var item in _createdComponents)
                {
                    yield return item;
                }
            }
        }
    }

    public T GetLastCreatedComponent<T>() where T: PlayerPrivateElementComponentBase
    {
        return (T)LastCreatedComponent;
    }

    public event Action<IScopedEntityFactory, PlayerPrivateElementComponentBase>? ComponentCreated;

    public ScopedEntityFactory(Entity entity, IElementCollection elementCollection, IEntityEngine entityEngine)
    {
        if (!entity.HasComponent<PlayerTagComponent>())
            throw new ArgumentException("Entity must be a player entity");

        _entity = entity;
        _elementCollection = elementCollection;
        _entityEngine = entityEngine;
        _entity.Disposed += HandleEntityDisposed;
    }

    private void HandleEntityDisposed(Entity obj)
    {
        Dispose();
    }

    private void Add(PlayerPrivateElementComponentBase playerPrivateElementComponentBase)
    {
        lock (_lock)
        {
            _lastCreatedComponent = playerPrivateElementComponentBase;
            _createdComponents.Add(playerPrivateElementComponentBase);
            ComponentCreated?.Invoke(this, playerPrivateElementComponentBase);
        }
    }

    private void AssociateWithPlayerEntity<TElementComponent>(PlayerPrivateElementComponent<TElementComponent> elementComponent, Entity playerEntity) where TElementComponent : ElementComponent
    {
        Add(elementComponent);
        var element = elementComponent.Element;
        var player = playerEntity.GetPlayer();
        element.AssociateWith(player);
        if (element is Pickup pickup)
        {
            pickup.CollisionShape.AssociateWith(player);
        }

        if (elementComponent.ElementComponent is MarkerElementComponent markerElementComponent)
        {
            markerElementComponent.CollisionShape.AssociateWith(player);
        }
    }

    public Entity CreateCollisionSphere(Vector3 position, float radius, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        ThrowIfDisposed();
        var collisionSphere = new CollisionSphere(position, radius)
        {
            Interior = interior,
            Dimension = dimension
        };

        var collisionSphereElementComponent = _entity.AddComponent(PlayerPrivateElementComponent.Create(new CollisionSphereElementComponent(collisionSphere, _entityEngine)));
        AssociateWithPlayerEntity(collisionSphereElementComponent, _entity);
        return _entity;

    }

    public Entity CreateMarker(MarkerType markerType, Vector3 position, Color color, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        ThrowIfDisposed();
        var marker = new Marker(position, markerType)
        {
            Color = color,
            Interior = interior,
            Dimension = dimension
        };

        var markerElementComponent = _entity.AddComponent(PlayerPrivateElementComponent.Create(new MarkerElementComponent(marker, _elementCollection, _entityEngine)));
        AssociateWithPlayerEntity(markerElementComponent, _entity);
        return _entity;
    }

    public Task<Entity> CreateNewPrivateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        ThrowIfDisposed();
        throw new NotImplementedException();
    }

    public Entity CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        ThrowIfDisposed();
        throw new NotImplementedException();
    }

    public Entity CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        ThrowIfDisposed();
        var worldObject = new WorldObject(model, position)
        {
            Rotation = rotation,
            Interior = interior,
            Dimension = dimension
        };

        var worldObjectElementComponent = _entity.AddComponent(PlayerPrivateElementComponent.Create(new WorldObjectComponent(worldObject)));
        AssociateWithPlayerEntity(worldObjectElementComponent, _entity);
        return _entity;
    }

    public Entity CreateBlip(BlipIcon blipIcon, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        ThrowIfDisposed();
        var blip = new Blip(position, blipIcon, 250)
        {
            Interior = interior,
            Dimension = dimension
        };


        var blipElementComponent = _entity.AddComponent(PlayerPrivateElementComponent.Create(new BlipElementComponent(blip)));
        AssociateWithPlayerEntity(blipElementComponent, _entity);
        return _entity;
    }

    public Entity CreatePickup(ushort model, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        ThrowIfDisposed();
        throw new NotImplementedException();
    }

    public Entity CreatePed(PedModel pedModel, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        ThrowIfDisposed();
        throw new NotImplementedException();
    }

    public Entity CreateRadarArea(Vector2 position, Vector2 size, Color color, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        ThrowIfDisposed();
        var radarArea = new RadarArea(position, size, color)
        {
            Interior = interior,
            Dimension = dimension
        };


        var radarAreaElementComponent = _entity.AddComponent(PlayerPrivateElementComponent.Create(new RadarAreaElementComponent(radarArea)));
        AssociateWithPlayerEntity(radarAreaElementComponent, _entity);
        return _entity;
    }

    public Entity CreateCollisionCircle(Vector2 position, float radius, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        ThrowIfDisposed();
        throw new NotImplementedException();
    }

    public Entity CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        ThrowIfDisposed();
        throw new NotImplementedException();
    }

    public Entity CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        ThrowIfDisposed();
        throw new NotImplementedException();
    }

    public Entity CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        ThrowIfDisposed();
        throw new NotImplementedException();
    }

    public Entity CreateCollisionTube(Vector3 position, float radius, float height, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        ThrowIfDisposed();
        throw new NotImplementedException();
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
