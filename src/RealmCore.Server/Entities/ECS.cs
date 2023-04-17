using System.Xml.Linq;

namespace RealmCore.Server.Entities;

internal sealed class ECS : IECS
{
    private readonly ReaderWriterLockSlim _entitiesLock = new();
    private readonly List<Entity> _entities = new();
    private readonly ConcurrentDictionary<Player, Entity> _entityByPlayer = new();
    private readonly ConcurrentDictionary<Element, Entity> _entityByElement = new();
    private readonly ConcurrentDictionary<string, Entity> _entityById = new();
    private readonly ConcurrentDictionary<string, Entity> _entityByName = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly IElementCollection _elementCollection;

    public IReadOnlyCollection<Entity> Entities
    {
        get
        {
            _entitiesLock.EnterWriteLock();
            var entities = new List<Entity>(_entities);
            _entitiesLock.ExitWriteLock();
            return entities;
        }
    }

    public IReadOnlyCollection<Entity> VehicleEntities
    {
        get
        {
            _entitiesLock.EnterWriteLock();
            var entities = new List<Entity>(_entities.Where(x => x.Tag == EntityTag.Vehicle));
            _entitiesLock.ExitWriteLock();
            return entities;
        }
    }

    public IReadOnlyCollection<Entity> PlayerEntities
    {
        get
        {
            _entitiesLock.EnterWriteLock();
            var entities = new List<Entity>(_entities.Where(x => x.Tag == EntityTag.Player));
            _entitiesLock.ExitWriteLock();
            return entities;
        }
    }

    public event Action<Entity>? EntityCreated;

    public ECS(IServiceProvider serviceProvider, IElementCollection elementCollection)
    {
        _serviceProvider = serviceProvider;
        _elementCollection = elementCollection;
    }

    public Entity GetEntityByPlayer(Player player)
    {
        return _entityByPlayer[player];
    }

    public Entity GetByElement(Element element)
    {
        return _entityByElement[element];
    }

    public bool TryGetEntityByPlayer(Player player, out Entity result)
    {
        if (player.IsDestroyed)
        {
            result = null;
            return false;
        }
        return _entityByPlayer.TryGetValue(player, out result);
    }

    public bool TryGetByElement(Element element, out Entity result)
    {
        if (element.IsDestroyed)
        {
            result = null;
            return false;
        }
        return _entityByElement.TryGetValue(element, out result);
    }

    private void InternalEntityCreated(Entity entity)
    {
        _entitiesLock.EnterWriteLock();
        try
        {
            _entities.Add(entity);
        }
        finally
        {
            _entitiesLock.ExitWriteLock();
        }
        _entityById[entity.Id] = entity;
        _entityByName[entity.Name] = entity;
        entity.ComponentAdded += HandleComponentAdded;
        entity.Disposed += HandleEntityDestroyed;
    }

    public Entity CreateEntity(string name, EntityTag tag, Action<Entity>? entityBuilder = null)
    {
        if (_entityByName.ContainsKey(name))
            throw new EntityAlreadyExistsException(name);

        var newlyCreatedEntity = new Entity(_serviceProvider, name, tag);

        InternalEntityCreated(newlyCreatedEntity);
        entityBuilder?.Invoke(newlyCreatedEntity);
        EntityCreated?.Invoke(newlyCreatedEntity);
        return newlyCreatedEntity;
    }

    private void HandleEntityDestroyed(Entity entity)
    {
        _entityById.Remove(entity.Id, out var _);
        _entityByName.Remove(entity.Name, out var _);

        _entitiesLock.EnterWriteLock();
        try
        {
            _entities.Remove(entity);
        }
        finally
        {
            _entitiesLock.ExitWriteLock();
        }

        entity.ComponentAdded -= HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is ElementComponent elementComponent)
        {
            _entityByElement[elementComponent.Element] = component.Entity;
            component.Entity.Disposed += HandleElementEntityDestroyed;
        }

        if (component is PlayerElementComponent playerElementComponent)
        {
            var player = playerElementComponent.Player;
            _entityByPlayer[playerElementComponent.Player] = component.Entity;
            component.Entity.Disposed += HandlePlayerEntityDestroyed;
        }
    }

    private void HandleElementEntityDestroyed(Entity elementEntity)
    {
        elementEntity.Disposed -= HandleElementEntityDestroyed;
        _entityByElement.Remove(elementEntity.Element, out var _);
    }

    private void HandlePlayerEntityDestroyed(Entity playerEntity)
    {
        playerEntity.Disposed -= HandlePlayerEntityDestroyed;
        _entityByPlayer.Remove(playerEntity.Player, out var _);
    }

    private void HandleDisposed(Entity entity)
    {
        _entityById.TryRemove(entity.Id, out var _);
        _entityByName.TryRemove(entity.Name, out var _);
        entity.Disposed -= HandleDisposed;
    }

    public bool GetEntityById(string id, out Entity? entity) => _entityById.TryGetValue(id, out entity);
    public bool GetEntityByName(string name, out Entity? entity) => _entityByName.TryGetValue(name, out entity);

    public IEnumerable<Entity> GetWithinRange(Vector3 position, float range)
    {
        var elements = _elementCollection.GetWithinRange(position, range);
        foreach (var element in elements)
        {
            if (TryGetByElement(element, out var entity))
                yield return entity;
        }
    }
}
