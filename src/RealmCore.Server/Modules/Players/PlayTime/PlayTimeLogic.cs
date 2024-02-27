namespace RealmCore.Server.Modules.Players.PlayTime;

internal sealed class PlayTimeLogic
{
    private readonly ISchedulerService _schedulerService;
    private readonly IElementCollection _elementCollection;

    public PlayTimeLogic(ISchedulerService schedulerService, IElementCollection elementCollection, ILogger<PlayTimeLogic> logger)
    {
        _schedulerService = schedulerService;
        _elementCollection = elementCollection;
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
}
