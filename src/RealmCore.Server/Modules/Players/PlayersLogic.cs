namespace RealmCore.Server.Modules.Players;

internal sealed class PlayersLogic : PlayerLifecycle
{
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly ILogger<PlayersLogic> _logger;
    private readonly IResourceProvider _resourceProvider;
    private readonly IUsersInUse _activeUsers;
    private readonly IPlayersEventManager _playerEventManager;
    private readonly IOptions<GuiBrowserOptions> _guiBrowserOptions;
    private readonly ClientConsole _clientConsole;
    private readonly IPlayersService _playersService;
    private readonly ConcurrentDictionary<RealmPlayer, Latch> _playerResources = new();

    public PlayersLogic(MtaServer server, IClientInterfaceService clientInterfaceService, ILogger<PlayersLogic> logger, IResourceProvider resourceProvider, IUsersInUse activeUsers, IPlayersEventManager playerEventManager, IOptions<GuiBrowserOptions> guiBrowserOptions, ClientConsole clientConsole, IPlayersService playersService) : base(server)
    {
        _clientInterfaceService = clientInterfaceService;
        _logger = logger;
        _resourceProvider = resourceProvider;
        _activeUsers = activeUsers;
        _playerEventManager = playerEventManager;
        _guiBrowserOptions = guiBrowserOptions;
        _clientConsole = clientConsole;
        _playersService = playersService;
    }

    private async Task FetchClientData(RealmPlayer player, CancellationToken cancellationToken = default)
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

        await StartAllResourcesForPlayer(player, cancellationToken);

        var timeoutTask = Task.Delay(10_000, cancellationToken);
        var completedTask = await Task.WhenAny(Task.WhenAll(taskWaitForScreenSize.Task, taskWaitForCultureInfo.Task), timeoutTask);
        cancellationToken.ThrowIfCancellationRequested();

        var screenSize = await taskWaitForScreenSize.Task;
        var cultureInfo = await taskWaitForCultureInfo.Task;

        player.ScreenSize = new Vector2(screenSize.Item1, screenSize.Item2);
        player.Culture = cultureInfo;
    }

    private async Task StartAllResourcesForPlayer(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        try
        {
            await _playerResources[player].WaitAsync(cancellationToken);
        }
        catch (Exception)
        {
            player.ResourceStarted -= HandlePlayerResourceStarted;
            _playerResources.TryRemove(player, out var _);
            throw;
        }
    }

    private async Task WaitForBrowser(RealmPlayer player, TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (player.Browser.IsReady)
            return;

        var waitForBrowser = new TaskCompletionSource();
        void handleBrowserReady(IPlayerBrowserFeature playerBrowser)
        {
            if (waitForBrowser.TrySetResult())
                player.Browser.Ready -= handleBrowserReady;
        }
        player.Browser.Ready += handleBrowserReady;

        if (player.Browser.IsReady)
        {
            if (waitForBrowser.TrySetResult())
                player.Browser.Ready -= handleBrowserReady;
        }

        if(!await waitForBrowser.WaitWithTimeout(timeout, cancellationToken))
        {
            throw new BrowserLoadingTimeoutException();
        }
    }

    private async Task HandlePlayerJoinedCore(RealmPlayer player, CancellationToken cancellationToken)
    {
        if (_playersService.TryGetPlayerByName(player.Name, out var foundPlayer, PlayerSearchOption.CaseInsensitive, player))
            throw new UserNameInUseException(player.Name);

        var start = Stopwatch.GetTimestamp();
        var resources = _resourceProvider.GetResources();
        _playerResources[player] = new Latch(RealmResourceServer._resourceCounter, TimeSpan.FromSeconds(60));
        player.ResourceStarted += HandlePlayerResourceStarted;
        player.Destroyed += HandlePlayerDestroyed;

        if (player.ScreenSize.X == 0)
        {
            using var activity = Activity.StartActivity("FetchClientData");
            await FetchClientData(player, cancellationToken);
        }
        else
        {
            using var activity = Activity.StartActivity("StartAllResourcesForPlayer");
            await StartAllResourcesForPlayer(player, cancellationToken);
        }

        if (_guiBrowserOptions.Value.BrowserSupport)
        {
            using var activity = Activity.StartActivity("WaitForBrowser");
            await WaitForBrowser(player, TimeSpan.FromSeconds(60), cancellationToken);
        }

        var stop = Stopwatch.GetTimestamp();
        double milliseconds = (stop - start) / (float)Stopwatch.Frequency * 1000;
        _logger.LogInformation("Player joined in {elapsedMilliseconds}ms", (ulong)milliseconds);

        if (player.IsDestroyed)
            return;
        player.Disconnected += HandleDisconnected;

        _playerEventManager.RelayLoaded(player);
    }


    private void HandleDisconnected(Player player, PlayerQuitEventArgs playerQuitEventArgs)
    {
        _logger.LogInformation("Player {playerName} disconnected", player.Name);
        player.Disconnected += HandleDisconnected;
    }

    protected override async void PlayerJoined(RealmPlayer player)
    {
        using var _ = _logger.BeginElement(player);
        using var handlePlayerJoinedActivity = Activity.StartActivity("PlayerJoined");
        try
        {
            await HandlePlayerJoinedCore(player, player.CreateCancellationToken());
        }
        catch (UserNameInUseException)
        {
            player.Kick("Nick jest używany przez innego gracza.");
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
            if (player.IsDestroyed || !player.Client.IsConnected)
            {
                return;
            }
            string what = "Wystąpił nieznany błąd.";
            if(ex is BrowserLoadingTimeoutException)
            {
                what = "Gui przeglądarki ładowało się zbyt długo.";
            }
            var traceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? "<nieznany>";
            var message = $"{what}. Jeżeli błąd się powtórzy zgłoś się do administracji.\n\nTrace id: {traceId}";
            _clientConsole.OutputTo(player, message);
            player.Kick(message);
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
            if (player.User.IsSignedIn)
            {
                if (_activeUsers.TrySetInactive(player.PersistentId))
                    _playerEventManager.RelayUnloading(player);

                await player.GetRequiredService<ISaveService>().Save();
            }
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

    public static readonly ActivitySource Activity = new("RealmCore.Players", "1.0.0");
}
