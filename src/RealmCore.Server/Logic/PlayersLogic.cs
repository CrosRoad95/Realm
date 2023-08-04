using RealmCore.Server.Utilities;
using SlipeServer.Server.Resources.Providers;

namespace RealmCore.Server.Logic;

internal class PlayersLogic
{
    private readonly IECS _ecs;
    private readonly IServiceProvider _serviceProvider;
    private readonly RealmDbContextFactory _realmDbContextFactory;
    private readonly MtaServer _mtaServer;
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ISaveService _saveService;
    private readonly ILogger<PlayersLogic> _logger;
    private readonly IResourceProvider _resourceProvider;
    private readonly ConcurrentDictionary<Player, Latch> _playerResources = new();

    public PlayersLogic(IECS ecs, IServiceProvider serviceProvider,
        RealmDbContextFactory realmDbContextFactory, MtaServer mtaServer, IClientInterfaceService clientInterfaceService, IDateTimeProvider dateTimeProvider, ISaveService saveService, ILogger<PlayersLogic> logger, IResourceProvider resourceProvider)
    {
        _ecs = ecs;
        _serviceProvider = serviceProvider;
        _realmDbContextFactory = realmDbContextFactory;
        _mtaServer = mtaServer;
        _clientInterfaceService = clientInterfaceService;
        _dateTimeProvider = dateTimeProvider;
        _saveService = saveService;
        _logger = logger;
        _resourceProvider = resourceProvider;
        _ecs.EntityCreated += HandleEntityCreated;
        _mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private async void HandlePlayerJoined(Player player)
    {
        var resources = _resourceProvider.GetResources();
        try
        {
            _playerResources[player] = new Latch(RealmResourceServer._resourceCounter, TimeSpan.FromSeconds(60));
            player.ResourceStarted += HandlePlayerResourceStarted;
            player.Disconnected += HandlePlayerDisconnected;

            var taskWaitForScreenSize = new TaskCompletionSource<(int, int)>();
            var taskWaitForCultureInfo = new TaskCompletionSource<CultureInfo>();
            void HandleClientScreenSizeChanged(Player player2, int x, int y)
            {
                if (player2 == player)
                {
                    _clientInterfaceService.ClientScreenSizeChanged -= HandleClientScreenSizeChanged;
                    taskWaitForScreenSize.SetResult((x, y));
                }
            }

            void HandleClientCultureInfoChanged(Player player2, CultureInfo cultureInfo)
            {
                if (player2 == player)
                {
                    _clientInterfaceService.ClientCultureInfoChanged -= HandleClientCultureInfoChanged;
                    taskWaitForCultureInfo.SetResult(cultureInfo);
                }
            }

            _clientInterfaceService.ClientScreenSizeChanged += HandleClientScreenSizeChanged;
            _clientInterfaceService.ClientCultureInfoChanged += HandleClientCultureInfoChanged;

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

            var screenSize = await taskWaitForScreenSize.Task;
            var cultureInfo = await taskWaitForCultureInfo.Task;

            _ecs.CreateEntity("Player " + player.Name, EntityTag.Player, entity =>
            {
                entity.AddComponent(new PlayerElementComponent(player, new Vector2(screenSize.Item1, screenSize.Item2), cultureInfo));
            });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle player joined");
        }
    }

    private void HandlePlayerResourceStarted(Player player, PlayerResourceStartedEventArgs e)
    {
        _playerResources[player].Decrement();
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (entity.Tag != EntityTag.Player)
            return;

        entity.Disposed += HandleEntityDestroyed;
        entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleEntityDestroyed(Entity entity)
    {
        entity.ComponentAdded -= HandleComponentAdded;
    }

    private async void HandleComponentAdded(Component component)
    {
        try
        {
            var entity = component.Entity;

            if (component is InventoryComponent inventoryComponent)
            {
                if (inventoryComponent.Id == 0)
                {
                    if (entity.TryGetComponent(out UserComponent userComponent))
                    {
                        var inventoryId = await _saveService.SaveNewPlayerInventory(inventoryComponent, userComponent.Id);
                        inventoryComponent.Id = inventoryId;
                    }
                }
            }

            {
                if (component is UserComponent userComponent)
                {
                    if (entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
                    {
                        var client = playerElementComponent.Player.Client;
                        using var context = _realmDbContextFactory.CreateDbContext();
                        var user = await context.Users.Where(x => x.Id == userComponent.Id).FirstAsync();
                        user.LastLogindDateTime = _dateTimeProvider.Now;
                        user.LastIp = client.IPAddress?.ToString();
                        user.LastSerial = client.Serial;
                        if (user.RegisterSerial == null)
                            user.RegisterSerial = client.Serial;

                        if (user.RegisterIp == null)
                            user.RegisterIp = user.LastIp;

                        if (user.RegisteredDateTime == null)
                            user.RegisteredDateTime = _dateTimeProvider.Now;
                        context.Users.Update(user);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong");
        }
    }

    private async void HandlePlayerDisconnected(Player player, PlayerQuitEventArgs e)
    {
        if (!_ecs.TryGetEntityByPlayer(player, out var playerEntity, true))
            return;
        try
        {
            player.Disconnected -= HandlePlayerDisconnected;
            _playerResources.TryRemove(player, out var _);
            try
            {
                var saveService = _serviceProvider.GetRequiredService<ISaveService>();
                await saveService.Save(playerEntity);
                await saveService.Commit();
            }
            finally
            {
                playerEntity.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle player disconnected");
            if (_ecs.ContainsEntity(playerEntity))
            {
                _logger.LogCritical(ex, "Failed to save and dispose entity! Executing backup disposing strategy.");
                try
                {
                    _ecs.RemoveEntity(playerEntity);
                }
                catch(Exception ex2)
                {
                    _logger.LogCritical(ex2, "Backup entity dispose strategy failed.");
                }
            }
        }
    }
}
