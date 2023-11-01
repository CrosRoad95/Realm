namespace RealmCore.Sample.Commands;

[CommandName("jobstatsall")]
public sealed class JobsStatsAllCommand : IInGameCommand
{
    private readonly ILogger<JobsStatsAllCommand> _logger;
    private readonly IJobService _jobService;
    private readonly ChatBox _chatBox;

    public JobsStatsAllCommand(ILogger<JobsStatsAllCommand> logger, IJobService jobService, ChatBox chatBox)
    {
        _logger = logger;
        _jobService = jobService;
        _chatBox = chatBox;
    }

    public async Task Handle(RealmPlayer player, CommandArguments args)
    {
        var stats = await _jobService.GetTotalJobStatistics(1);
        foreach (var item in stats)
        {
            _chatBox.OutputTo(player, $"stats {item.Key}: {item.Value.points}, time: {item.Value.timePlayed}");
        }
    }
}
