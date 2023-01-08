using Microsoft.AspNetCore.Identity;
using Realm.Domain.Interfaces;
using Realm.Persistance.Data;

namespace Realm.Server.Logic;

internal class SaveEntitiesLogic
{
    private readonly ECS _ecs;
    private readonly ILoadAndSaveService _loadAndSaveService;
    private readonly IEntityByElement _entityByElement;
    private readonly RealmDbContextFactory _realmDbContextFactory;

    public SaveEntitiesLogic(ECS ecs, ILoadAndSaveService loadAndSaveService, IEntityByElement entityByElement, RealmDbContextFactory realmDbContextFactory)
    {
        _ecs = ecs;
        _loadAndSaveService = loadAndSaveService;
        _entityByElement = entityByElement;
        _realmDbContextFactory = realmDbContextFactory;
        _ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        entity.Destroyed += HandleEntityDestroyed;
        entity.ComponentAdded += HandleComponentAdded;
    }

    private Task HandleEntityDestroyed(Entity entity)
    {
        entity.ComponentAdded -= HandleComponentAdded;
        return Task.CompletedTask;
    }

    private async void HandleComponentAdded(Component component)
    {
        var entity = component.Entity;
        {
            if (component is PlayerElementComponent playerElementComponent)
            {
                var player = playerElementComponent.Player;
                player.Disconnected += HandlePlayerDisconnected;
            }
        }
        if (component is AccountComponent accountComponent)
        {
            if (entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
            {
                var client = playerElementComponent.Player.Client;
                using var context = _realmDbContextFactory.CreateDbContext();
                var user = await context.Users.Where(x => x.Id == accountComponent.Id).FirstAsync();
                user.LastLogindDateTime = DateTime.Now;
                user.LastIp = client.IPAddress?.ToString();
                user.LastSerial = client.Serial;
                if (user.RegisterSerial == null)
                    user.RegisterSerial = client.Serial;

                if (user.RegisterIp == null)
                    user.RegisterIp = user.LastIp;

                if (user.RegisteredDateTime == null)
                    user.RegisteredDateTime = DateTime.Now;
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
        }
    }

    private async void HandlePlayerDisconnected(Player player, SlipeServer.Server.Elements.Events.PlayerQuitEventArgs e)
    {
        using var context = _realmDbContextFactory.CreateDbContext();
        var playerEntity = _entityByElement.GetByElement(player) ?? throw new InvalidOperationException();
        await _loadAndSaveService.Save(playerEntity, context);
        await playerEntity.Destroy();
    }
}
