namespace RealmCore.Server.Entities;

internal sealed class EntityEngine : IEntityEngine
{
    private readonly ReaderWriterLockSlim _entitiesLock = new();
    private readonly List<Entity> _entities = new();
    private readonly ConcurrentDictionary<string, Entity> _entityById = new();
    private readonly ConcurrentDictionary<int, Entity> _vehicleById = new();

    public IReadOnlyCollection<Entity> Entities
    {
        get
        {
            _entitiesLock.EnterWriteLock();
            var entities = new List<Entity>(_entities);
            _entitiesLock.ExitWriteLock();
            return entities.AsReadOnly();
        }
    }

    public int EntitiesCount
    {
        get
        {
            _entitiesLock.EnterWriteLock();
            var count = _entities.Count;
            _entitiesLock.ExitWriteLock();
            return count;
        }
    }
    
    public int EntitiesComponentsCount
    {
        get
        {
            _entitiesLock.EnterWriteLock();
            var count = _entities.Sum(x => x.ComponentsCount);
            _entitiesLock.ExitWriteLock();
            return count;
        }
    }

    public IEnumerable<Entity> GetEntitiesContainingComponent<TComponent>() where TComponent : Component
    {
        _entitiesLock.EnterReadLock();
        foreach (var entity in _entities)
        {
            if (entity.HasComponent<TComponent>())
            {
                yield return entity;
            }
        }
        _entitiesLock.ExitReadLock();
    }

    public IEnumerable<Entity> VehicleEntities => GetEntitiesContainingComponent<VehicleTagComponent>();
    public IEnumerable<Entity> PlayerEntities => GetEntitiesContainingComponent<PlayerTagComponent>();

    public event Action<Entity>? EntityCreated;

    public bool ContainsEntity(Entity entity)
    {
        return _entities.Contains(entity);
    }

    private void InternalEntityCreated(Entity entity)
    {
        _entitiesLock.EnterWriteLock();
        try
        {
            _entities.Add(entity);
            _entityById[entity.Id] = entity;
            entity.Disposed += HandleEntityDisposed;
        }
        finally
        {
            _entitiesLock.ExitWriteLock();
        }
    }

    public Entity CreateEntity(Action<Entity>? entityBuilder = null)
    {
        var newlyCreatedEntity = new Entity();

        InternalEntityCreated(newlyCreatedEntity);
        entityBuilder?.Invoke(newlyCreatedEntity);
        EntityCreated?.Invoke(newlyCreatedEntity);
        return newlyCreatedEntity;
    }

    private void HandleEntityDisposed(Entity entity)
    {
        RemoveEntity(entity);
    }

    public void RemoveEntity(Entity entity)
    {
        _entityById.Remove(entity.Id, out var _);

        _entitiesLock.EnterWriteLock();
        try
        {
            _entities.Remove(entity);
        }
        finally
        {
            _entitiesLock.ExitWriteLock();
        }
    }

    private void HandleDisposed(Entity entity)
    {
        _entityById.TryRemove(entity.Id, out var _);
        entity.Disposed -= HandleDisposed;
    }

    public bool GetEntityById(string id, out Entity? entity) => _entityById.TryGetValue(id, out entity);
    public bool GetVehicleById(int id, out Entity? entity) => _vehicleById.TryGetValue(id, out entity);
}
