﻿using RealmCore.Server.Components.Elements.Abstractions;

namespace RealmCore.Server.Entities;

internal sealed class EntityEngine : IEntityEngine
{
    private readonly ReaderWriterLockSlim _entitiesLock = new();
    private readonly List<Entity> _entities = new();
    private readonly ConcurrentDictionary<Player, Entity> _entityByPlayer = new();
    private readonly ConcurrentDictionary<Element, Entity> _entityByElement = new();
    private readonly ConcurrentDictionary<string, Entity> _entityById = new();
    private readonly ConcurrentDictionary<string, Entity> _entityByName = new();
    private readonly ConcurrentDictionary<int, Entity> _vehicleById = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly IElementCollection _elementCollection;
    private readonly ILogger<EntityEngine> _logger;

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
    public Entity Console { get; }

    public EntityEngine(IServiceProvider serviceProvider, IElementCollection elementCollection, ILogger<EntityEngine> logger)
    {
        _serviceProvider = serviceProvider;
        _elementCollection = elementCollection;
        _logger = logger;
        Console = CreateEntity("console", e =>
        {
            e.AddComponent<ConsoleTagComponent>();
        });
    }

    public bool ContainsEntity(Entity entity)
    {
        return _entities.Contains(entity);
    }

    public Entity GetEntityByPlayer(Player player)
    {
        return _entityByPlayer[player];
    }

    public Entity GetByElement(Element element)
    {
        return _entityByElement[element];
    }

    public bool TryGetEntityByPed(Ped ped, out Entity result, bool ignoreDestroyed = false)
    {
        if (ped.IsDestroyed && !ignoreDestroyed)
        {
            result = null;
            return false;
        }
        if(ped is Player player)
            return _entityByPlayer.TryGetValue(player, out result);
        return _entityByElement.TryGetValue(ped, out result);
    }
    
    public bool TryGetEntityByPlayer(Player player, out Entity result, bool ignoreDestroyed = false)
    {
        if (player.IsDestroyed && !ignoreDestroyed)
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
        entity.Disposed += HandleEntityDisposed;
    }

    public Entity CreateEntity(string name, Action<Entity>? entityBuilder = null)
    {
        if (_entityByName.ContainsKey(name))
            throw new EntityAlreadyExistsException(name);

        var newlyCreatedEntity = new Entity(name);

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
            component.Entity.PreDisposed += HandleElementEntityPreDisposed;
        }

        if (component is PlayerElementComponent playerElementComponent)
        {
            var player = playerElementComponent.Player;
            _entityByPlayer[playerElementComponent.Player] = component.Entity;
            component.Entity.PreDisposed += HandleElementEntityPreDisposed;
        }

        switch(component)
        {
            case PrivateVehicleComponent vehicleComponent:
                if(_vehicleById.TryAdd(vehicleComponent.Id, component.Entity))
                {
                    component.Entity.PreDisposed += HandleElementEntityPreDisposed;
                }
                else
                {
                    _logger.LogWarning("Duplicated private vehicle component {vehicleId}", vehicleComponent.Id);
                }
                break;
        }
    }

    private void HandleVehicleEntityPreDisposed(Entity entity)
    {
        _vehicleById.TryRemove(entity.GetRequiredComponent<PrivateVehicleComponent>().Id, out _);
        entity.PreDisposed += HandleVehicleEntityPreDisposed;
    }

    private void HandleElementEntityPreDisposed(Entity elementEntity)
    {
        _entityByElement.Remove(elementEntity.GetElement(), out var _);
        elementEntity.PreDisposed -= HandleElementEntityPreDisposed;
    }

    private void HandlePlayerEntityDestroyed(Entity playerEntity)
    {
        playerEntity.Disposed -= HandlePlayerEntityDestroyed;
        _entityByPlayer.Remove(playerEntity.GetPlayer(), out var _);
    }

    private void HandleDisposed(Entity entity)
    {
        _entityById.TryRemove(entity.Id, out var _);
        _entityByName.TryRemove(entity.Name, out var _);
        entity.Disposed -= HandleDisposed;
    }

    public bool GetEntityById(string id, out Entity? entity) => _entityById.TryGetValue(id, out entity);
    public bool GetEntityByName(string name, out Entity? entity) => _entityByName.TryGetValue(name, out entity);
    public bool GetVehicleById(int id, out Entity? entity) => _vehicleById.TryGetValue(id, out entity);

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