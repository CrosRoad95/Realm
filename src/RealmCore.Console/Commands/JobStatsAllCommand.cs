namespace RealmCore.Console.Commands;

[CommandName("jobstatsall")]
public sealed class JobsStatsAllCommand : IIngameCommand
{
    private readonly ILogger<JobsStatsAllCommand> _logger;
    private readonly IJobService _jobService;

    public JobsStatsAllCommand(ILogger<JobsStatsAllCommand> logger, IJobService jobService)
    {
        _logger = logger;
        _jobService = jobService;
    }

    public async Task Handle(Entity entity, string[] args)
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        var stats = await _jobService.GetTotalJobStatistics(1);
        foreach (var item in stats)
        {
            playerElementComponent.SendChatMessage($"stats {item.Key}: {item.Value.points}, time: {item.Value.timePlayed}");
        }
    }
}
