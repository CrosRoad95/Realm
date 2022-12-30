using Microsoft.AspNetCore.Identity;
using Realm.Domain.Components.Common;
using Realm.Domain.Components.Vehicles;
using Realm.Domain.Interfaces;
using Realm.Domain.Inventory;
using Realm.Persistance.Data;
using System.Linq;

namespace Realm.Server;

public sealed class ECS : IEntityByElement
{
    private readonly List<Entity> _entities = new();
    private readonly Dictionary<Player, Entity> _entityByPlayer = new();
    private readonly Dictionary<Element, Entity> _entityByElement = new();
    private readonly Dictionary<string, Entity> _entityByName = new();
    private readonly RPGServer _server;
    private readonly UserManager<User> _userManager;
    public IEnumerable<Entity> Entities => _entities;

    public event Action<Entity>? EntityCreated;
    public ECS(RPGServer server, UserManager<User> userManager)
    {
        _server = server;
        _userManager = userManager;
    }

    public Entity GetEntityByPlayer(Player player)
    {
        return _entityByPlayer[player];
    }

    public Entity GetByElement(Element element)
    {
        return _entityByElement[element];
    }

    public Entity CreateEntity(string name, string tag, Action<Entity>? entityBuilder = null)
    {
        if (_entityByName.ContainsKey(name))
            throw new Exception($"Entity of name {name} already exists");

        var newlyCreatedEntity = new Entity(_server, _server.GetRequiredService<ServicesComponent>(), name, tag);
        _entities.Add(newlyCreatedEntity);
        _entityByName[name] = newlyCreatedEntity;
        if (entityBuilder != null)
            entityBuilder(newlyCreatedEntity);
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

    public async ValueTask<bool> Save(Entity entity)
    {
        var context = entity.GetRequiredService<IDb>();
        switch (entity.Tag)
        {
            case Entity.PlayerTag:
                if (entity.TryGetComponent(out AccountComponent accountComponent))
                {
                    var user = accountComponent.User;
                    
                    if (entity.TryGetComponent(out LicensesComponent licensesComponent))
                        user.Licenses = licensesComponent.Licenses;

                    if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
                    {
                        bool updateInventory = true;
                        if (user.Inventory == null)
                        {
                            user.Inventory = new Inventory
                            {
                                Size = inventoryComponent.Size,
                                Id = inventoryComponent.Id,
                                InventoryItems = new List<InventoryItem>()
                            };
                            updateInventory = false;
                            context.Inventories.Add(user.Inventory);
                        }

                        user.Inventory.Size = inventoryComponent.Size;
                        user.Inventory.InventoryItems = inventoryComponent.Items.Select(item => new InventoryItem
                        {
                            Id = item.Id ?? Guid.NewGuid().ToString(),
                            Inventory = user.Inventory,
                            InventoryId = user.Inventory.Id,
                            ItemId = item.ItemId,
                            Number = item.Number,
                            MetaData = JsonConvert.SerializeObject(item.MetaData, Formatting.None),
                        }).ToList();
                        if(updateInventory)
                        {
                            user.Inventory.Id = inventoryComponent.Id;
                            context.Inventories.Update(user.Inventory);
                        }
                    }
                    user.LastTransformAndMotion = entity.Transform.GetTransformAndMotion();
                    await _userManager.UpdateAsync(user);
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
            try
            {

            await Save(entity);
            }
            catch(Exception ex)
            {
                ;
                throw;
            }
        }
    }
}
