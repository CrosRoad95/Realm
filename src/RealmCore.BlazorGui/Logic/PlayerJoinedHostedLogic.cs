namespace RealmCore.BlazorGui.Logic;

internal sealed class PlayerJoinedHostedLogic : IHostedService
{
    private readonly ILogger<PlayerJoinedHostedLogic> _logger;
    private readonly INametagsService _nametagsService;
    private readonly ChatBox _chatBox;
    private readonly Text3dService _text3DService;
    private readonly PlayersEventManager _playersEventManager;
    private readonly IUsersService _usersService;
    private readonly ScoreboardService _scoreboardService;

    public PlayerJoinedHostedLogic(ILogger<PlayerJoinedHostedLogic> logger, INametagsService nametagsService, ChatBox chatBox, Text3dService text3DService, PlayersEventManager playersEventManager, IUsersService usersService, ScoreboardService scoreboardService)
    {
        _logger = logger;
        _nametagsService = nametagsService;
        _chatBox = chatBox;
        _text3DService = text3DService;
        _playersEventManager = playersEventManager;
        _usersService = usersService;
        _scoreboardService = scoreboardService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _playersEventManager.Loaded += HandlePlayerLoaded;
        _playersEventManager.Unloading += HandlePlayerUnloading;
        _usersService.LoggedIn += HandleLoggedIn;
        _usersService.LoggedOut += HandleLoggedOut;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _playersEventManager.Loaded -= HandlePlayerLoaded;
        _playersEventManager.Unloading -= HandlePlayerUnloading;
        _usersService.LoggedIn -= HandleLoggedIn;
        _usersService.LoggedOut -= HandleLoggedOut;
        return Task.CompletedTask;
    }

    private void HandlePlayerUnloading(Player obj)
    {
        if (obj is RealmPlayer player)
        {
            player.Events.Add(42);
        }
    }

    private void HandleReady(IPlayerBrowserFeature playerBrowser)
    {
        _chatBox.OutputTo(playerBrowser.Player, "Browser ready");
        //playerBrowser.DevTools = true;
        ShowLoginSequence(playerBrowser.Player);
    }

    private async Task HandleLoggedIn(RealmPlayer player)
    {
        await player.FadeCameraAsync(CameraFade.Out);
        _chatBox.SetVisibleFor(player, true);
        _chatBox.ClearFor(player);
        player.Gui.TryClose();
        player.Camera.Target = player;
        if (!player.TrySpawnAtLastPosition())
        {
            player.Spawn(new Vector3(362.58f + (float)Random.Shared.NextDouble() * 3, -91.07f + (float)Random.Shared.NextDouble() * 3, 1.38f),
                new Vector3(0, 0, 90));
        }
        await Task.Delay(300);
        await player.FadeCameraAsync(CameraFade.In);
        _text3DService.SetRenderingEnabled(player, true);

        _nametagsService.SetNametagRenderingEnabled(player, true);
        _scoreboardService.SetEnabledTo(player, true);

        if (player.Settings.TryGet(69, out var name))
        {
            _chatBox.OutputTo(player, $"Witaj {name}");
        }
    }

    private Task HandleLoggedOut(RealmPlayer player)
    {
        ShowLoginSequence(player);
        return Task.CompletedTask;
    }

    private void ShowLoginSequence(RealmPlayer player)
    {
        _scoreboardService.SetEnabledTo(player, false);
        _text3DService.SetRenderingEnabled(player, false);
        _chatBox.SetVisibleFor(player, false);
        _chatBox.ClearFor(player);
        player.Camera.Fade(CameraFade.In);
        player.Camera.SetMatrix(new Vector3(379.89844f, -92.6416f, 10.950561f), new Vector3(336.75684f, -93.018555f, 1.3956465f));
        var admin = player.Admin;
        admin.DebugView = true;
        admin.DevelopmentMode = true;
        admin.SetTools(new List<AdminTool> { AdminTool.Elements, AdminTool.ShowSpawnMarkers });

        if (player.Gui.Current == null)
        {
            //player.Gui.Current = new Counter1Gui(player);
            player.Gui.SetCurrentWithDI<LoginGui>();
        }
    }

    private void HandlePlayerLoaded(Player plr)
    {
        if (plr is RealmPlayer player)
        {
            player.Browser.Ready += HandleReady;

            ShowLoginSequence(player);
            player.Level.Changed += HandleLevelChanged;
        }
    }

    private void HandleLevelChanged(IPlayerLevelFeature levelService, uint level, LevelChange levelChange)
    {
        _logger.LogInformation("Player level changed to: {level}, change: {levelChange}", level, levelChange);
    }
}
