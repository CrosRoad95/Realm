﻿namespace RealmCore.Server.Modules.Players;

internal sealed class PlayersHostedService : PlayerLifecycle, IHostedService
{
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly ILogger<PlayersHostedService> _logger;
    private readonly IResourceProvider _resourceProvider;
    private readonly UsersInUse _activeUsers;
    private readonly ClientConsole _clientConsole;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IRealmResourcesProvider _realmResourcesProvider;
    private readonly IBrowserService _browserService;
    private readonly ConcurrentDictionary<RealmPlayer, Latch> _playerResources = new();

    public PlayersHostedService(PlayersEventManager playersEventManager, IClientInterfaceService clientInterfaceService, ILogger<PlayersHostedService> logger, IResourceProvider resourceProvider, UsersInUse activeUsers, ClientConsole clientConsole, IHostEnvironment hostEnvironment, IRealmResourcesProvider realmResourcesProvider, IBrowserService browserService) : base(playersEventManager)
    {
        _clientInterfaceService = clientInterfaceService;
        _logger = logger;
        _resourceProvider = resourceProvider;
        _activeUsers = activeUsers;
        _clientConsole = clientConsole;
        _hostEnvironment = hostEnvironment;
        _realmResourcesProvider = realmResourcesProvider;
        _browserService = browserService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
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
        void handleBrowserReady(PlayerBrowserFeature playerBrowser)
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

        _browserService.Load(player, player.ScreenSize);

        if (!await waitForBrowser.WaitWithTimeout(timeout, cancellationToken))
        {
            throw new BrowserLoadingTimeoutException();
        }
    }

    private async Task HandlePlayerJoinedCore(RealmPlayer player, CancellationToken cancellationToken)
    {
        var elementSearchService = player.GetRequiredService<PlayerSearchService>();
        if (elementSearchService.TryGetPlayerByName(player.Name, out var foundPlayer, PlayerSearchOption.CaseInsensitive, player))
            throw new UserNameInUseException(player.Name);

        var start = Stopwatch.GetTimestamp();
        if (_realmResourcesProvider.Count == 0)
            throw new InvalidOperationException("No resources found");

        _playerResources[player] = new Latch(_realmResourcesProvider.Count - player.StartedResources.Count, TimeSpan.FromSeconds(60));
        player.ResourceStarted += HandlePlayerResourceStarted;

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

        {
            using var activity = Activity.StartActivity("WaitForBrowser");
            await WaitForBrowser(player, TimeSpan.FromSeconds(60), cancellationToken);
        }

        var stop = Stopwatch.GetTimestamp();
        double milliseconds = (stop - start) / (float)Stopwatch.Frequency * 1000;

        if (player.IsDestroyed)
        {
            _logger.LogInformation("Player joined in {elapsedMilliseconds}ms but player is destroyed", (ulong)milliseconds);
            return;
        }

        _logger.LogInformation("Player joined in {elapsedMilliseconds}ms", (ulong)milliseconds);
        player.Disconnected += HandleDisconnected;
        player.Spawned += HandleSpawned;

        _playersEventManager.RelayLoaded(player);
    }

    private void HandleSpawned(Player sender, PlayerSpawnedEventArgs e)
    {
        _playersEventManager.RelaySpawned((RealmPlayer)e.Source);
    }

    private async void HandleDisconnected(Player plr, PlayerQuitEventArgs playerQuitEventArgs)
    {
        var player = (RealmPlayer)plr;
        try
        {
            _logger.LogInformation("Player {playerName} disconnected", player.Name);
            player.Disconnected -= HandleDisconnected;
            player.Spawned -= HandleSpawned;
            _playerResources.TryRemove(player, out var _);
            if (player.User.IsLoggedIn)
            {
                if (_activeUsers.TrySetInactive(player.UserId))
                    _playersEventManager.RelayUnloading(player);
            }
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
        }

        try
        {
            await player.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
        }
    }

    private async Task PlayerJoinedCore(RealmPlayer player)
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
            string what = "Wystąpił nieznany błąd";
            if (ex is BrowserLoadingTimeoutException)
            {
                what = "Gui przeglądarki ładowało się zbyt długo";
            }

            var traceId = handlePlayerJoinedActivity?.Id ?? "<nieznany>";
            var message = $"{what}. Jeżeli błąd się powtórzy zgłoś się do administracji.\n\nTrace id: {traceId}";
            if (_hostEnvironment.IsDevelopment())
            {
                _clientConsole.OutputTo(player, "Exception message:");
                _clientConsole.OutputTo(player, ex.ToString());
                _clientConsole.OutputTo(player, "");
            }
            _clientConsole.OutputTo(player, message);
            player.Kick(message);
        }
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        Task.Run(async () =>
        {
            try
            {
                await PlayerJoinedCore(player);
            }
            catch(Exception ex)
            {
                _logger.LogHandleError(ex);
            }
        });
    }

    private void HandlePlayerResourceStarted(Player player, PlayerResourceStartedEventArgs e)
    {
        if (_playerResources.TryGetValue((RealmPlayer)player, out var latch))
            latch.Decrement();
    }

    public static readonly ActivitySource Activity = new("RealmCore.Players", "1.0.0");
}
