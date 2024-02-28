namespace RealmCore.Server.Modules.Players;

public interface IPlayerSchedulerFeature : IPlayerFeature, ISchedulerService
{

}

internal sealed class PlayerSchedulerFeature : IPlayerSchedulerFeature, IDisposable
{
    private readonly ISchedulerService _schedulerService;
    private readonly CancellationTokenSource _cts = new();

    public RealmPlayer Player { get; init; }

    public PlayerSchedulerFeature(PlayerContext playerContext, ISchedulerService schedulerService)
    {
        Player = playerContext.Player;
        _schedulerService = schedulerService;
    }

    public Task StartAsync() => throw new NotSupportedException();

    public ScheduledJob ScheduleJob(Func<Task> job, TimeSpan every, CancellationToken cancellationToken = default)
    {
        _cts.Token.ThrowIfCancellationRequested();

        return _schedulerService.ScheduleJob(job, every, CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken).Token);
    }

    public ScheduledJobAt ScheduleJobAt(Func<Task> job, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        _cts.Token.ThrowIfCancellationRequested();

        return _schedulerService.ScheduleJobAt(job, dateTime, CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken).Token);
    }

    public CronScheduledJob ScheduleJobAt(Func<Task> job, string cronExpression, CancellationToken cancellationToken = default)
    {
        _cts.Token.ThrowIfCancellationRequested();

        return _schedulerService.ScheduleJobAt(job, cronExpression, CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken).Token);
    }

    public void ScheduleJobOnce(Func<Task> job, TimeSpan delay, CancellationToken cancellationToken = default)
    {
        _cts.Token.ThrowIfCancellationRequested();

        _schedulerService.ScheduleJobOnce(job, delay, CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken).Token);
    }

    public void Dispose()
    {
        _cts.Cancel();
    }
}
