using Microsoft.AspNetCore.Identity;
using Realm.Domain.Components.Vehicles;
using Realm.Persistance.Data;

namespace Realm.Server;

public sealed class ECS
{
    private readonly List<Entity> _entities = new();
    private readonly Dictionary<Player, Entity> _entityByPlayer = new();
    private readonly Dictionary<string, Entity> _entityByName = new();
    private readonly RPGServer _server;
    private readonly UserManager<User> _userManager;
    public IEnumerable<Entity> Entities => _entities;

    public event Action<Entity> EntityCreated;
    public ECS(RPGServer server, UserManager<User> userManager)
    {
        _server = server;
        _userManager = userManager;
    }

    public Entity GetEntityByPlayer(Player player)
    {
        return _entityByPlayer[player];
    }

    public Entity CreateEntity(string name, string tag)
    {
        if (_entityByName.ContainsKey(name))
            throw new Exception($"Entity of name {name} already exists");

        var newlyCreatedEntity = new Entity(_server, _server.GetRequiredService<ServicesComponent>(), name, tag);
        _entities.Add(newlyCreatedEntity);
        _entityByName[name] = newlyCreatedEntity;
        newlyCreatedEntity.ComponentAdded += HandleComponentAdded;
        newlyCreatedEntity.Destroyed += HandleEntityDestroyed;
        EntityCreated?.Invoke(newlyCreatedEntity);
        return newlyCreatedEntity;
    }

    private async void HandleEntityDestroyed(Entity entity)
    {
        await Save(entity);
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

    public async ValueTask<bool> Save(Entity entity)
    {
        switch (entity.Tag)
        {
            case Entity.PlayerTag:
                if (entity.TryGetComponent(out AccountComponent accountComponent))
                {
                    accountComponent.User.LastTransformAndMotion = entity.Transform.GetTransformAndMotion();
                    await _userManager.UpdateAsync(accountComponent.User);
                    return true;
                }
                break;
            case Entity.VehicleTag:
                if (entity.TryGetComponent(out PersistantVehicleComponent persistantVehicleComponent))
                {
                    ;
                }
                break;
        }
        return false;
    }

    public async Task SaveAll()
    {
        foreach (var entity in _entities)
        {
            await Save(entity);
        }
    }
}
