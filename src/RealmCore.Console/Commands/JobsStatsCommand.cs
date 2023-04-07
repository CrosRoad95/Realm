namespace RealmCore.Console.Commands;

[CommandName("jobstats")]
public sealed class JobsStatsCommand : IIngameCommand
{
    private readonly ILogger<JobsStatsCommand> _logger;
    private readonly IJobService _jobService;

    public JobsStatsCommand(ILogger<JobsStatsCommand> logger, IJobService jobService)
    {
        _logger = logger;
        _jobService = jobService;
    }

    public async Task Handle(Entity entity, string[] args)
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        var userComponent = entity.GetRequiredComponent<UserComponent>();
        var stats = await _jobService.TryGetTotalUserJobStatistics(userComponent.Id, 1);
        playerElementComponent.SendChatMessage($"stats: {stats.Value.points}, time: {stats.Value.timePlayed}");
    }
}
