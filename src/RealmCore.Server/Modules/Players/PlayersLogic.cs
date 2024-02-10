namespace RealmCore.Server.Modules.Players;

internal sealed class PlayersLogic
{
    private readonly MtaServer _mtaServer;
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly ILogger<PlayersLogic> _logger;
    private readonly IResourceProvider _resourceProvider;
    private readonly IUsersInUse _activeUsers;
    private readonly IPlayerEventManager _playersService;
    private readonly ConcurrentDictionary<RealmPlayer, Latch> _playerResources = new();

    public PlayersLogic(IElementFactory elementFactory, MtaServer mtaServer, IClientInterfaceService clientInterfaceService, ILogger<PlayersLogic> logger, IResourceProvider resourceProvider, IUsersInUse activeUsers, IPlayerEventManager playersService)
    {
        _mtaServer = mtaServer;
        _clientInterfaceService = clientInterfaceService;
        _logger = logger;
        _resourceProvider = resourceProvider;
        _activeUsers = activeUsers;
        _playersService = playersService;
        _mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private async Task FetchClientData(RealmPlayer player)
    {
        var taskWaitForScreenSize = new TaskCompletionSource<(int, int)>();
        var taskWaitForCultureInfo = new TaskCompletionSource<CultureInfo>();
        void handleClientScreenSizeChanged(Player player2, int x, int y)
        {
            if (player2 == player)
            {
                _clientInterfaceService.ClientScreenSizeChanged -= handleClientScreenSizeChanged;
                taskWaitForScreenSize.SetResult((x, y));
            }
        }

        void handleClientCultureInfoChanged(Player player2, CultureInfo cultureInfo)
        {
            if (player2 == player)
            {
                _clientInterfaceService.ClientCultureInfoChanged -= handleClientCultureInfoChanged;
                taskWaitForCultureInfo.SetResult(cultureInfo);
            }
        }

        _clientInterfaceService.ClientScreenSizeChanged += handleClientScreenSizeChanged;
        _clientInterfaceService.ClientCultureInfoChanged += handleClientCultureInfoChanged;

        await StartAllResourcesForPlayer(player);

        var timeoutTask = Task.Delay(10_000);
        var completedTask = await Task.WhenAny(Task.WhenAll(taskWaitForScreenSize.Task, taskWaitForCultureInfo.Task), timeoutTask);
        if (completedTask == timeoutTask)
        {
            player.Kick("Failed to get culture and screen size");
            return;
        }
        var screenSize = await taskWaitForScreenSize.Task;
        var cultureInfo = await taskWaitForCultureInfo.Task;

        player.ScreenSize = new Vector2(screenSize.Item1, screenSize.Item2);
        player.CultureInfo = cultureInfo;
    }

    private async Task StartAllResourcesForPlayer(RealmPlayer player)
    {
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

    }

    private async Task HandlePlayerJoinedCore(Player plr)
    {
        var player = (RealmPlayer)plr;
        var start = Stopwatch.GetTimestamp();
        var resources = _resourceProvider.GetResources();
        _playerResources[player] = new Latch(RealmResourceServer._resourceCounter, TimeSpan.FromSeconds(60));
        player.ResourceStarted += HandlePlayerResourceStarted;
        player.Destroyed += HandlePlayerDestroyed;

        if (player.ScreenSize.X == 0)
        {
            await FetchClientData(player);
        }
        else
        {
            await StartAllResourcesForPlayer(player);
        }

        var stop = Stopwatch.GetTimestamp();
        double milliseconds = (stop - start) / (float)Stopwatch.Frequency * 1000;
        _logger.LogInformation("Player joined in {elapsedMilliseconds}ms", (ulong)milliseconds);

        if (player.IsDestroyed)
            return;
        player.Disconnected += HandleDisconnected;

        _playersService.RelayLoaded(player);
    }


    private void HandleDisconnected(Player player, PlayerQuitEventArgs playerQuitEventArgs)
    {
        _logger.LogInformation("Player {playerName} disconnected", player.Name);
        player.Disconnected += HandleDisconnected;
    }

    private async void HandlePlayerJoined(Player player)
    {
        try
        {
        using var _ = _logger.BeginElement(player);
            await HandlePlayerJoinedCore(player);
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
        }
    }

    private void HandlePlayerResourceStarted(Player player, PlayerResourceStartedEventArgs e)
    {
        if (_playerResources.TryGetValue((RealmPlayer)player, out var latch))
            latch.Decrement();
    }

    private async void HandlePlayerDestroyed(Element plr)
    {
        var player = (RealmPlayer)plr;
        try
        {
            plr.Destroyed -= HandlePlayerDestroyed;
            _playerResources.TryRemove(player, out var _);
            _activeUsers.TrySetInactive(player.UserId);
            await player.GetRequiredService<ISaveService>().Save(player);
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
        }

        try
        {
            player.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
        }
    }
}
