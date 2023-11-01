namespace RealmCore.Sample.Commands;

[CommandName("licenses")]
public sealed class LicensesCommand : IInGameCommand
{
    private readonly ILogger<LicensesCommand> _logger;
    private readonly ChatBox _chatBox;

    public LicensesCommand(ILogger<LicensesCommand> logger, ChatBox chatBox)
    {
        _logger = logger;
        _chatBox = chatBox;
    }

    public Task Handle(RealmPlayer player, CommandArguments args)
    {
        if (player.TryGetComponent(out LicensesComponent licenseComponent))
        {
            _chatBox.OutputTo(player, $"Licenses");
            foreach (var license in licenseComponent.Licenses)
            {
                _chatBox.OutputTo(player, $"License: {license.licenseId} = {license.IsSuspended}");
            }

        }
        return Task.CompletedTask;
    }
}
