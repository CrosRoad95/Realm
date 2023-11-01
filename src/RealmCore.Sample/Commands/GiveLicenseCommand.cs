namespace RealmCore.Sample.Commands;

[CommandName("givelicense")]
public sealed class GiveLicenseCommand : IInGameCommand
{
    private readonly ILogger<GiveLicenseCommand> _logger;
    private readonly ChatBox _chatBox;

    public GiveLicenseCommand(ILogger<GiveLicenseCommand> logger, ChatBox chatBox)
    {
        _logger = logger;
        _chatBox = chatBox;
    }

    public Task Handle(RealmPlayer player, CommandArguments args)
    {
        if (player.TryGetComponent(out LicensesComponent licenseComponent))
        {
            var license = args.ReadInt();
            if (licenseComponent.TryAddLicense(license))
            {
                _chatBox.OutputTo(player, $"license added: '{license}'");
            }
            else
            {
                _chatBox.OutputTo(player, $"failed to add license: '{license}'");
            }
        }
        return Task.CompletedTask;
    }
}
