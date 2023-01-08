using Realm.Domain.Interfaces;

namespace Realm.Server.Logic;

internal class PlayersLogic
{
    private readonly ECS _ecs;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityByElement _entityByElement;
    private readonly RealmDbContextFactory _realmDbContextFactory;
    private readonly MtaServer _mtaServer;

    public PlayersLogic(ECS ecs, IServiceProvider serviceProvider, IEntityByElement entityByElement,
        RealmDbContextFactory realmDbContextFactory, MtaServer mtaServer)
    {
        _ecs = ecs;
        _serviceProvider = serviceProvider;
        _entityByElement = entityByElement;
        _realmDbContextFactory = realmDbContextFactory;
        _mtaServer = mtaServer;
        _ecs.EntityCreated += HandleEntityCreated;
        _mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player player)
    {
        _ecs.CreateEntity("Player " + player.Name, Entity.PlayerTag, entity =>
        {
            entity.AddComponent(new PlayerElementComponent(player));
        });
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
        var saveService = _serviceProvider.GetRequiredService<ISaveService>();
        var playerEntity = _entityByElement.GetByElement(player) ?? throw new InvalidOperationException();
        await saveService.Save(playerEntity);
        await saveService.Commit();
        await playerEntity.Destroy();
    }
}
