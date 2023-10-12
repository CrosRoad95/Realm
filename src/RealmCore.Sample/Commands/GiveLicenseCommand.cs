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

    public Task Handle(Entity entity, CommandArguments args)
    {
        if (entity.TryGetComponent(out LicensesComponent licenseComponent))
        {
            var license = args.ReadInt();
            if (licenseComponent.TryAddLicense(license))
            {
                _chatBox.OutputTo(entity, $"license added: '{license}'");
            }
            else
            {
                _chatBox.OutputTo(entity, $"failed to add license: '{license}'");
            }
        }
        return Task.CompletedTask;
    }
}
