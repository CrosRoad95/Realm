using Realm.Domain.Exceptions;
using System.Collections.Concurrent;

namespace Realm.Server;

public sealed class ECS : IEntityByElement
{
    private readonly ReaderWriterLockSlim _entitiesLock = new();
    private readonly List<Entity> _entities = new();
    private readonly ConcurrentDictionary<Player, Entity> _entityByPlayer = new();
    private readonly ConcurrentDictionary<Element, Entity> _entityByElement = new();
    private readonly ConcurrentDictionary<string, Entity> _entityByName = new();
    private readonly IServiceProvider _serviceProvider;

    public IReadOnlyCollection<Entity> Entities => _entities;

    public event Action<Entity>? EntityCreated;
    public ECS(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IReadOnlyList<Entity> GetPlayerEntities() => new List<Entity>(_entityByPlayer.Values);

    public Entity GetEntityByPlayer(Player player)
    {
        return _entityByPlayer[player];
    }

    public Entity GetByElement(Element element)
    {
        return _entityByElement[element];
    }

    public Entity? TryGetEntityByPlayer(Player player)
    {
        _entityByPlayer.TryGetValue(player, out Entity? result);
        return result;
    }

    public Entity? TryGetByElement(Element element)
    {
        _entityByElement.TryGetValue(element, out Entity? result);
        return result;
    }
    
    public Entity CreateEntity(string name, Entity.EntityTag tag, Action<Entity>? entityBuilder = null)
    {
        if (_entityByName.ContainsKey(name))
            throw new EntityAlreadyExistsException(name);

        var newlyCreatedEntity = new Entity(_serviceProvider, name, tag);
        _entitiesLock.EnterWriteLock();
        try
        {
            _entities.Add(newlyCreatedEntity);
        }
        finally
        {
            _entitiesLock.ExitWriteLock();
        }
        _entityByName[name] = newlyCreatedEntity;
        newlyCreatedEntity.ComponentAdded += HandleComponentAdded;
        newlyCreatedEntity.Destroyed += HandleEntityDestroyed;
        if (entityBuilder != null)
            entityBuilder(newlyCreatedEntity);
        EntityCreated?.Invoke(newlyCreatedEntity);
        return newlyCreatedEntity;
    }

    private void HandleEntityDestroyed(Entity entity)
    {
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

    public void Destroy(Entity entity)
    {
        entity.Dispose();
    }

    private void HandleComponentAdded(Component component)
    {
        if(component is ElementComponent elementComponent)
        {
            _entityByElement[elementComponent.Element] = component.Entity;
            component.Entity.ComponentDetached += HandleElementComponentRemoved;
        }

        if (component is PlayerElementComponent playerElementComponent)
        {
            var player = playerElementComponent.Player;
            _entityByPlayer[playerElementComponent.Player] = component.Entity;
            component.Entity.Destroyed += HandlePlayerEntityDestroyed;
        }
    }

    private void HandleElementComponentRemoved(Component component)
    {
        if (component is ElementComponent elementComponent)
            _entityByElement.Remove(elementComponent.Element, out var _);
    }

    private void HandlePlayerEntityDestroyed(Entity playerEntity)
    {
        playerEntity.Destroyed -= HandlePlayerEntityDestroyed;
        var playerComponent = playerEntity.GetRequiredComponent<PlayerElementComponent>();
        _entityByPlayer.Remove(playerComponent.Player, out var _);
    }
}
