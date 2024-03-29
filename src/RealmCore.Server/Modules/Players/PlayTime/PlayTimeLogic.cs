﻿namespace RealmCore.Server.Modules.Players.PlayTime;

internal sealed class PlayTimeLogic : PlayerLifecycle
{
    private readonly ISchedulerService _schedulerService;
    private readonly IElementCollection _elementCollection;
    private readonly IDateTimeProvider _dateTimeProvider;

    public PlayTimeLogic(MtaServer mtaServer, ISchedulerService schedulerService, IElementCollection elementCollection, ILogger<PlayTimeLogic> logger, IDateTimeProvider dateTimeProvider) : base(mtaServer)
    {
        _schedulerService = schedulerService;
        _elementCollection = elementCollection;
        _dateTimeProvider = dateTimeProvider;
        try
        {
            _schedulerService.ScheduleJob(UpdatePlayTime, TimeSpan.FromSeconds(1), CancellationToken.None);
        }
        catch(Exception ex)
        {
            logger.LogHandleError(ex);
        }
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
