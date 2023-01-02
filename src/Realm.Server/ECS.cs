using Realm.Domain.Interfaces;

namespace Realm.Server;

public sealed class ECS : IEntityByElement
{
    private readonly List<Entity> _entities = new();
    private readonly Dictionary<Player, Entity> _entityByPlayer = new();
    private readonly Dictionary<Element, Entity> _entityByElement = new();
    private readonly Dictionary<string, Entity> _entityByName = new();
    private readonly RPGServer _server;
    public IEnumerable<Entity> Entities => _entities;

    public event Action<Entity>? EntityCreated;
    public ECS(RPGServer server)
    {
        _server = server;
    }

    public Entity GetEntityByPlayer(Player player)
    {
        return _entityByPlayer[player];
    }

    public Entity GetByElement(Element element)
    {
        return _entityByElement[element];
    }

    public Entity? TryGetByElement(Element element)
    {
        Entity? result;
        _entityByElement.TryGetValue(element, out result);
        return result;
    }

    public async Task<Entity> CreateEntityAsync(string name, string tag, Func<Entity, Task>? entityBuilder = null)
    {
        if (_entityByName.ContainsKey(name))
            throw new Exception($"Entity of name {name} already exists");

        var newlyCreatedEntity = new Entity(_server, name, tag);
        _entities.Add(newlyCreatedEntity);
        _entityByName[name] = newlyCreatedEntity;
        if (entityBuilder != null)
            await entityBuilder(newlyCreatedEntity);
        newlyCreatedEntity.ComponentAdded += HandleComponentAdded;
        newlyCreatedEntity.Destroyed += HandleEntityDestroyed;
        EntityCreated?.Invoke(newlyCreatedEntity);
        return newlyCreatedEntity;
    }
    
    public Entity CreateEntity(string name, string tag, Action<Entity>? entityBuilder = null)
    {
        if (_entityByName.ContainsKey(name))
            throw new Exception($"Entity of name {name} already exists");

        var newlyCreatedEntity = new Entity(_server, name, tag);
        _entities.Add(newlyCreatedEntity);
        _entityByName[name] = newlyCreatedEntity;
        if (entityBuilder != null)
            entityBuilder(newlyCreatedEntity);
        newlyCreatedEntity.ComponentAdded += HandleComponentAdded;
        newlyCreatedEntity.Destroyed += HandleEntityDestroyed;
        EntityCreated?.Invoke(newlyCreatedEntity);
        return newlyCreatedEntity;
    }

    private void HandleEntityDestroyed(Entity entity)
    {
        _entityByName.Remove(entity.Name);
        _entities.Remove(entity);

        entity.ComponentAdded -= HandleComponentAdded;
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
            player.Disconnected += HandlePlayerDisconnected;
        }
    }

    private void HandleElementComponentRemoved(Component component)
    {
        if (component is not ElementComponent elementComponent)
            return;
        _entityByElement.Remove(elementComponent.Element);
    }

    private void HandlePlayerDisconnected(Player player, SlipeServer.Server.Elements.Events.PlayerQuitEventArgs e)
    {
        var playerEntity = _entityByPlayer[player];
        playerEntity.Destroy();
    }

    private void HandlePlayerEntityDestroyed(Entity playerEntity)
    {
        playerEntity.Destroyed += HandlePlayerEntityDestroyed;
        var playerComponent = playerEntity.GetRequiredComponent<PlayerElementComponent>();
        _entityByPlayer.Remove(playerComponent.Player);
    }
}
