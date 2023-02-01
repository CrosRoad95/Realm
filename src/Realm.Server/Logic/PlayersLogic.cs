using Realm.Common.Utilities;
using System.Collections.Concurrent;

namespace Realm.Server.Logic;

internal class PlayersLogic
{
    const int RESOURCES_COUNT = 10;
    private readonly ECS _ecs;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityByElement _entityByElement;
    private readonly RealmDbContextFactory _realmDbContextFactory;
    private readonly MtaServer _mtaServer;
    private readonly ConcurrentDictionary<Player, Latch> _playerResources = new();

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

    private async void HandlePlayerJoined(Player player)
    {
        _playerResources[player] = new Latch(RESOURCES_COUNT, TimeSpan.FromSeconds(60));
        player.ResourceStarted += HandlePlayerResourceStarted;
        player.Disconnected += HandlePlayerDisconnected;
        try
        {
            await _playerResources[player].WaitAsync();
        }
        catch (Exception)
        {
            player.Kick("Resources took to long to load. Please reconnect.");
            return;
        }
        finally
        {
            player.ResourceStarted -= HandlePlayerResourceStarted;
            _playerResources.TryRemove(player, out var _);
        }

        _ecs.CreateEntity("Player " + player.Name, Entity.EntityTag.Player, entity =>
        {
            entity.AddComponent(new PlayerElementComponent(player));
        });
    }

    private void HandlePlayerResourceStarted(Player player, SlipeServer.Server.Elements.Events.PlayerResourceStartedEventArgs e)
    {
        _playerResources[player].Decrement();
    }

    private void HandleEntityCreated(Entity entity)
    {
        entity.Destroyed += HandleEntityDestroyed;
        entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleEntityDestroyed(Entity entity)
    {
        entity.ComponentAdded -= HandleComponentAdded;
    }

    private async void HandleComponentAdded(Component component)
    {
        var entity = component.Entity;
        {
            if (component is PlayerElementComponent playerElementComponent)
            {
                var player = playerElementComponent.Player;
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
        player.Disconnected -= HandlePlayerDisconnected;
        _playerResources.TryRemove(player, out var _);
        var playerEntity = _entityByElement.TryGetEntityByPlayer(player);
        if(playerEntity != null)
        {
            var saveService = _serviceProvider.GetRequiredService<ISaveService>();
            await saveService.Save(playerEntity);
            await saveService.Commit();
            playerEntity.Dispose();
        }
    }
}
