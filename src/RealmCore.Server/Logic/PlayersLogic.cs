namespace RealmCore.Server.Logic;

internal sealed class PlayersLogic
{
    private readonly IElementFactory _elementFactory;
    private readonly MtaServer _mtaServer;
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly ILogger<PlayersLogic> _logger;
    private readonly IResourceProvider _resourceProvider;
    private readonly ConcurrentDictionary<Player, Latch> _playerResources = new();

    public PlayersLogic(IElementFactory elementFactory, MtaServer mtaServer, IClientInterfaceService clientInterfaceService, ILogger<PlayersLogic> logger, IResourceProvider resourceProvider)
    {
        _elementFactory = elementFactory;
        _mtaServer = mtaServer;
        _clientInterfaceService = clientInterfaceService;
        _logger = logger;
        _resourceProvider = resourceProvider;
        _mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private async Task HandlePlayerJoinedCore(Player plr)
    {
        var player = (RealmPlayer)plr;
        var start = Stopwatch.GetTimestamp();
        var resources = _resourceProvider.GetResources();
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

        player.ScreenSize = new Vector2(screenSize.Item1, screenSize.Item2);
        player.CultureInfo = cultureInfo;

        var stop = Stopwatch.GetTimestamp();
        double milliseconds = ((stop - start) / (float)Stopwatch.Frequency) * 1000;
        _elementFactory.RelayCreated(player);
        _logger.LogInformation("Player joined in {elapsedMilliseconds}", milliseconds);

        if (player.IsDestroyed)
            return;
        player.Disconnected += HandleDisconnected;
        player.VehicleChanged += HandleVehicleChanged;
    }

    private void HandleVehicleChanged(Ped sender, ElementChangedEventArgs<Ped, Vehicle?> args)
    {
        throw new NotImplementedException();
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
        _playerResources[player].Decrement();
    }

    private async void HandlePlayerDisconnected(Player plr, PlayerQuitEventArgs e)
    {
        try
        {
            var player = (RealmPlayer)plr;
            plr.Disconnected -= HandlePlayerDisconnected;
            _playerResources.TryRemove(plr, out var _);
            await player.ServiceProvider.GetRequiredService<ISaveService>().Save(player);
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
        }
    }
}
