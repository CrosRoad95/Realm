namespace RealmCore.Server.Modules.Server;

internal sealed class SchedulerLogic
{
    public SchedulerLogic(ISchedulerService schedulerService)
    {
        Task.Run(schedulerService.StartAsync);
    }
}
