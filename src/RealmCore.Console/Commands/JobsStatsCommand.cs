using RealmCore.Server.Extensions;
using SlipeServer.Server.Services;

namespace RealmCore.Console.Commands;

[CommandName("jobstats")]
public sealed class JobsStatsCommand : IIngameCommand
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

    public async Task Handle(Entity entity, string[] args)
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        var userComponent = entity.GetRequiredComponent<UserComponent>();
        var stats = await _jobService.TryGetTotalUserJobStatistics(userComponent.Id, 1);
        _chatBox.OutputTo(entity, $"stats: {stats.Value.points}, time: {stats.Value.timePlayed}");
    }
}
