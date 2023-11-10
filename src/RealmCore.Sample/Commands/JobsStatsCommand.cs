namespace RealmCore.Sample.Commands;

[CommandName("jobstats")]
public sealed class JobsStatsCommand : IInGameCommand
{
    private readonly ILogger<JobsStatsCommand> _logger;
    private readonly ChatBox _chatBox;

    public JobsStatsCommand(ILogger<JobsStatsCommand> logger, ChatBox chatBox)
    {
        _logger = logger;
        _chatBox = chatBox;
    }

    public async Task Handle(RealmPlayer player, CommandArguments args)
    {
        var stats = player.JobStatistics.GetTotalPoints(1);
        _chatBox.OutputTo(player, $"stats: {stats.Item1}, time: {stats.Item2}");
    }
}
