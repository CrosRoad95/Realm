using RealmCore.Server.Components.Elements.Abstractions;
using RealmCore.Server.Interfaces;

namespace RealmCore.Server.Entities;

internal sealed class ScopedEntityFactory : IScopedEntityFactory
{
    private readonly Entity _entity;
    private readonly IElementCollection _elementCollection;
    private readonly IEntityFactory _entityFactory;
    private readonly IEntityEngine _entityEngine;
    private PlayerPrivateElementComponentBase? _lastCreatedComponent;
    private bool _disposed;
    public event Action<ScopedEntityFactory>? Disposed;
    private readonly object _lock = new();
    private readonly List<PlayerPrivateElementComponentBase> _createdComponents = new();

    public Entity Entity => _entity;
    public PlayerPrivateElementComponentBase? LastCreatedComponent => _lastCreatedComponent;
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

    public event Action<IScopedEntityFactory, PlayerPrivateElementComponentBase>? ComponentCreated;

    public ScopedEntityFactory(Entity entity, IElementCollection elementCollection, IEntityFactory entityFactory, IEntityEngine entityEngine)
    {
        if (!entity.HasComponent<PlayerTagComponent>())
            throw new ArgumentException("Entity must be a player entity");

        _entity = entity;
        _elementCollection = elementCollection;
        _entityFactory = entityFactory;
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

    private void AssociateWithPlayerEntity(ElementComponent elementComponent, Entity playerEntity)
    {
        var element = elementComponent.Element;

        element.Id = (ElementId)playerEntity.GetRequiredComponent<PlayerElementComponent>().MapIdGenerator.GetId();
        var player = playerEntity.GetPlayer();
        element.AssociateWith(player);
        if (element is Pickup pickup)
        {
            pickup.CollisionShape.AssociateWith(player);
        }
        if (elementComponent is MarkerElementComponent markerElementComponent)
        {
            markerElementComponent.CollisionShape.AssociateWith(player);
        }
    }


    public Entity CreateCollisionSphere(Vector3 position, float radius, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        throw new NotImplementedException();
    }

    public Entity CreateMarker(MarkerType markerType, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        var marker = new Marker(position, markerType)
        {
            Color = Color.White
        };

        var markerElementComponent = _entity.AddComponent(PlayerPrivateElementComponent.Create(new MarkerElementComponent(marker, _elementCollection, _entityEngine)));
        AssociateWithPlayerEntity(markerElementComponent, _entity);
        return _entity;
    }

    public Task<Entity> CreateNewPrivateVehicle(ushort model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        throw new NotImplementedException();
    }

    public Entity CreateVehicle(ushort model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        throw new NotImplementedException();
    }

    public Entity CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        var worldObject = new WorldObject(model, position)
        {
            Rotation = rotation
        };

        var worldObjectElementComponent = _entity.AddComponent(PlayerPrivateElementComponent.Create(new WorldObjectComponent(worldObject)));
        AssociateWithPlayerEntity(worldObjectElementComponent, _entity);
        return _entity;
    }

    public Entity CreateBlip(BlipIcon blipIcon, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        throw new NotImplementedException();
    }

    public Entity CreatePickup(ushort model, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        throw new NotImplementedException();
    }

    public Entity CreatePed(PedModel pedModel, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        throw new NotImplementedException();
    }

    public Entity CreateRadarArea(Vector2 position, Vector2 size, Color color, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        throw new NotImplementedException();
    }

    public Entity CreateCollisionCircle(Vector2 position, float radius, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        throw new NotImplementedException();
    }

    public Entity CreateCollisionCuboid(Vector3 position, Vector3 dimensions, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        throw new NotImplementedException();
    }

    public Entity CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        throw new NotImplementedException();
    }

    public Entity CreateCollisionRectangle(Vector2 position, Vector2 dimensions, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        throw new NotImplementedException();
    }

    public Entity CreateCollisionTube(Vector3 position, float radius, float height, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        throw new NotImplementedException();
    }

    public PlayerPrivateElementComponent<BlipElementComponent> CreateBlipFor(Entity playerEntity, BlipIcon blipIcon, Vector3 position)
    {
        if (!playerEntity.HasComponent<PlayerTagComponent>())
            throw new ArgumentException("Entity must be a player entity");

        var blip = new Blip(position, blipIcon, 250);

        var blipElementComponent = playerEntity.AddComponent(PlayerPrivateElementComponent.Create(new BlipElementComponent(blip)));
        AssociateWithPlayerEntity(blipElementComponent, playerEntity);
        return blipElementComponent;
    }


    public PlayerPrivateElementComponent<RadarAreaElementComponent> CreateRadarAreaFor(Entity playerEntity, Vector2 position, Vector2 size, Color color)
    {
        if (!playerEntity.HasComponent<PlayerTagComponent>())
            throw new ArgumentException("Entity must be a player entity");

        var radarArea = new RadarArea(position, size, color);

        var radarAreaElementComponent = playerEntity.AddComponent(PlayerPrivateElementComponent.Create(new RadarAreaElementComponent(radarArea)));
        AssociateWithPlayerEntity(radarAreaElementComponent, playerEntity);
        return radarAreaElementComponent;
    }

    //public PlayerPrivateElementComponent<CollisionSphereElementComponent> CreateCollisionSphereFor(Entity playerEntity, Vector3 position, float radius)
    //{
    //    if (!playerEntity.HasComponent<PlayerTagComponent>())
    //        throw new ArgumentException("Entity must be a player entity");

    //    var collisionSphere = new CollisionSphere(position, radius);

    //    var collisionSphereElementComponent = playerEntity.AddComponent(PlayerPrivateElementComponent.Create(new CollisionSphereElementComponent(collisionSphere)));
    //    AssociateWithPlayerEntity(collisionSphereElementComponent, playerEntity);
    //    return collisionSphereElementComponent;
    //}

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
