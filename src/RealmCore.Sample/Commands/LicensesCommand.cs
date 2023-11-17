namespace RealmCore.Sample.Commands;

[CommandName("licenses")]
public sealed class LicensesCommand : IInGameCommand
{
    private readonly ILogger<LicensesCommand> _logger;
    private readonly ChatBox _chatBox;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LicensesCommand(ILogger<LicensesCommand> logger, ChatBox chatBox, IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _chatBox = chatBox;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        _chatBox.OutputTo(player, $"Licenses");
        foreach (var license in player.Licenses)
        {
            _chatBox.OutputTo(player, $"License: {license.LicenseId} = {license.IsSuspended(_dateTimeProvider.Now)}");
        }

        return Task.CompletedTask;
    }
}
