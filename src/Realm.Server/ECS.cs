namespace Realm.Server;

public sealed class ECS
{
    private readonly List<Entity> _entities = new();
    private readonly Dictionary<Player, Entity> _entityByPlayer = new();
    private readonly Dictionary<string, Entity> _entityByName = new();
    private readonly RPGServer _server;

    public IEnumerable<Entity> Entities => _entities;

    public ECS(RPGServer server)
    {
        _server = server;
    }

    public Entity GetEntityByPlayer(Player player)
    {
        return _entityByPlayer[player];
    }

    public Entity CreateEntity(string name)
    {
        if (_entityByName.ContainsKey(name))
            throw new Exception($"Entity of name {name} already exists");

        var newlyCreatedEntity = new Entity(_server, _server.GetRequiredService<ServicesComponent>(), name);
        _entities.Add(newlyCreatedEntity);
        _entityByName[name] = newlyCreatedEntity;
        newlyCreatedEntity.ComponentAdded += HandleComponentAdded;
        newlyCreatedEntity.Destroyed += HandleEntityDestroyed;
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
        if (component is PlayerElementComponent playerElementComponent)
        {
            var player = playerElementComponent.Player;
            _entityByPlayer[playerElementComponent.Player] = component.Entity;
            component.Entity.Destroyed += HandlePlayerEntityDestroyed;
            player.Disconnected += HandlePlayerDisconnected;
        }
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
