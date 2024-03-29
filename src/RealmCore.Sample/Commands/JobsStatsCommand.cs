﻿namespace RealmCore.Sample.Commands;

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
        var stats = player.JobStatistics.GetTotalPoints(1);
        _chatBox.OutputTo(player, $"stats: {stats.Item1}, time: {stats.Item2}");
        return Task.CompletedTask;
    }
}
