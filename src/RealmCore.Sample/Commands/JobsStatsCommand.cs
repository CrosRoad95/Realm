namespace RealmCore.Sample.Commands;

[CommandName("jobstats")]
public sealed class JobsStatsCommand : IInGameCommand
{
    private readonly ILogger<JobsStatsCommand> _logger;
    private readonly ChatBox _chatBox;

    public string[] RequiredPolicies { get; } = [];
    public JobsStatsCommand(ILogger<JobsStatsCommand> logger, ChatBox chatBox)
    {
        _logger = logger;
        _chatBox = chatBox;
    }

    public Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        var summary = player.JobStatistics.GetSummary(1);
        _chatBox.OutputTo(player, $"points: {summary.points}, time: {summary.timePlayed}");
        return Task.CompletedTask;
    }
}
