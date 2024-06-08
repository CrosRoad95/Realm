namespace RealmCore.BlazorGui.Commands;

[CommandName("addpoints")]
public sealed class AddPointsCommand : IInGameCommand
{
    private readonly ILogger<AddPointsCommand> _logger;
    private readonly ChatBox _chatBox;

    public string[] RequiredPolicies { get; } = [];
    public AddPointsCommand(ILogger<AddPointsCommand> logger, ChatBox chatBox)
    {
        _logger = logger;
        _chatBox = chatBox;
    }

    public Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        player.JobStatistics.AddPoints(1, 1);
        _chatBox.OutputTo(player, "added 1 point to job id 1");
        return Task.CompletedTask;
    }
}
