namespace Realm.Console.Commands;

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

    public async Task Handle(Guid traceId, Entity entity, string[] args)
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        var accountComponent = entity.GetRequiredComponent<AccountComponent>();
        var stats = await _jobService.GetTotalUserJobStatistics(accountComponent.Id, 1);
        playerElementComponent.SendChatMessage($"stats: {stats.points}, time: {stats.timePlayed}");
    }
}
