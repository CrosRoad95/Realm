namespace RealmCore.Sample.Commands;

[CommandName("jobstats")]
public sealed class JobsStatsCommand : IInGameCommand
{
    private readonly ILogger<JobsStatsCommand> _logger;
    private readonly IJobService _jobService;
    private readonly ChatBox _chatBox;

    public JobsStatsCommand(ILogger<JobsStatsCommand> logger, IJobService jobService, ChatBox chatBox)
    {
        _logger = logger;
        _jobService = jobService;
        _chatBox = chatBox;
    }

    public async Task Handle(RealmPlayer player, CommandArguments args)
    {
        var stats = await _jobService.TryGetTotalUserJobStatistics(player.GetUserId(), 1);
        _chatBox.OutputTo(player, $"stats: {stats.Value.points}, time: {stats.Value.timePlayed}");
    }
}
