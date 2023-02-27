namespace Realm.Console.Commands;

[CommandName("addpoints")]
public sealed class AddPointsCommand : IIngameCommand
{
    private readonly ILogger<AddPointsCommand> _logger;

    public AddPointsCommand(ILogger<AddPointsCommand> logger)
    {
        _logger = logger;
    }

    public Task Handle(Guid traceId, Entity entity, string[] args)
    {
        entity.GetRequiredComponent<JobStatisticsComponent>().AddPoints(1, 1);
        entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage("added 1 point to job id 1");
        return Task.CompletedTask;
    }
}
