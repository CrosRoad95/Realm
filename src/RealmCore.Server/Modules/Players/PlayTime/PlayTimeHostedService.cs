namespace RealmCore.Server.Modules.Players.PlayTime;

internal sealed class PlayTimeHostedService : PlayerLifecycle, IHostedService
{
    private readonly ISchedulerService _schedulerService;
    private readonly IElementCollection _elementCollection;
    private readonly ILogger<PlayTimeHostedService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public PlayTimeHostedService(PlayersEventManager playersEventManager, ISchedulerService schedulerService, IElementCollection elementCollection, ILogger<PlayTimeHostedService> logger, IDateTimeProvider dateTimeProvider) : base(playersEventManager)
    {
        _schedulerService = schedulerService;
        _elementCollection = elementCollection;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _schedulerService.ScheduleJob(UpdatePlayTime, TimeSpan.FromSeconds(1), CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task UpdatePlayTime()
    {
        foreach (var player in _elementCollection.GetByType<RealmPlayer>())
        {
            if(player.IsSpawned)
                player.PlayTime.Update();
        }
        return Task.CompletedTask;
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.PlayTime.CategoryChanged += HandleCategoryChanged;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        player.PlayTime.CategoryChanged -= HandleCategoryChanged;
    }

    private void HandleCategoryChanged(IPlayerPlayTimeFeature playerPlayTime, int? previous, int? current)
    {
        playerPlayTime.Player.PlayTime.UpdateCategoryPlayTime(previous);
    }
}
