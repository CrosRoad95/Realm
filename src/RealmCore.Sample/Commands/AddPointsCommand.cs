namespace RealmCore.Sample.Commands;

[CommandName("addpoints")]
public sealed class AddPointsCommand : IInGameCommand
{
    private readonly ILogger<AddPointsCommand> _logger;
    private readonly ChatBox _chatBox;

    public AddPointsCommand(ILogger<AddPointsCommand> logger, ChatBox chatBox)
    {
        _logger = logger;
        _chatBox = chatBox;
    }

    public Task Handle(Entity entity, CommandArguments args)
    {
        entity.GetRequiredComponent<JobStatisticsComponent>().AddPoints(1, 1);
        _chatBox.OutputTo(entity, "added 1 point to job id 1");
        return Task.CompletedTask;
    }
}
