using Realm.Domain.Exceptions;
using Realm.Domain.Interfaces;

namespace Realm.Server;

public sealed class ECS : IEntityByElement
{
    private readonly List<Entity> _entities = new();
    private readonly Dictionary<Player, Entity> _entityByPlayer = new();
    private readonly Dictionary<Element, Entity> _entityByElement = new();
    private readonly Dictionary<string, Entity> _entityByName = new();
    private readonly IServiceProvider _serviceProvider;

    public IEnumerable<Entity> Entities => _entities;

    public event Action<Entity>? EntityCreated;
    public ECS(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<Entity> GetPlayerEntities() => _entityByPlayer.Values;

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
    
    public Entity CreateEntity(string name, string tag, Action<Entity>? entityBuilder = null)
    {
        if (_entityByName.ContainsKey(name))
            throw new EntityAlreadyExistsException(name);

        var newlyCreatedEntity = new Entity(_serviceProvider, name, tag);
        _entities.Add(newlyCreatedEntity);
        _entityByName[name] = newlyCreatedEntity;
        if (entityBuilder != null)
            entityBuilder(newlyCreatedEntity);
        newlyCreatedEntity.ComponentAdded += HandleComponentAdded;
        newlyCreatedEntity.Destroyed += HandleEntityDestroyed;
        EntityCreated?.Invoke(newlyCreatedEntity);
        return newlyCreatedEntity;
    }

    private Task HandleEntityDestroyed(Entity entity)
    {
        _entityByName.Remove(entity.Name);
        _entities.Remove(entity);

        entity.ComponentAdded -= HandleComponentAdded;
        return Task.CompletedTask;
    }

    public Task Destroy(Entity entity)
    {
        return entity.Destroy();
    }

    private void HandleComponentAdded(Component component)
    {
        if(component is ElementComponent elementComponent)
        {
            _entityByElement[elementComponent.Element] = component.Entity;
            component.Entity.ComponentRemoved += HandleElementComponentRemoved;
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
            _entityByElement.Remove(elementComponent.Element);
    }

    private Task HandlePlayerEntityDestroyed(Entity playerEntity)
    {
        playerEntity.Destroyed += HandlePlayerEntityDestroyed;
        var playerComponent = playerEntity.GetRequiredComponent<PlayerElementComponent>();
        _entityByPlayer.Remove(playerComponent.Player);
        return Task.CompletedTask;
    }
}
