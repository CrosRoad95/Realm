﻿namespace RealmCore.Sample.Commands;

[CommandName("jobstatsall")]
public sealed class JobsStatsAllCommand : IInGameCommand
{
    private readonly ChatBox _chatBox;

    public string[] RequiredPolicies { get; } = [];
    public JobsStatsAllCommand(ChatBox chatBox)
    {
        _chatBox = chatBox;
    }

    public async Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        var stats = player.JobStatistics.GetTotalPoints(1);
        _chatBox.OutputTo(player, $"stats, points: {stats.Item1}, time: {stats.Item2}");
    }
}
