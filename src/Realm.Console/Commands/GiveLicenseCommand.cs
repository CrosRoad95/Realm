namespace Realm.Console.Commands;

[CommandName("givelicense")]
public sealed class GiveLicenseCommand : IIngameCommand
{
    private readonly ILogger<GiveLicenseCommand> _logger;

    public GiveLicenseCommand(ILogger<GiveLicenseCommand> logger)
    {
        _logger = logger;
    }

    public Task Handle(Entity entity, string[] args)
    {
        if (entity.TryGetComponent(out LicensesComponent licenseComponent))
        {
            var license = args.First();
            if (licenseComponent.AddLicense(int.Parse(license)))
            {
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"license added: '{license}'");
            }
            else
            {
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"failed to add license: '{license}'");
            }
        }
        return Task.CompletedTask;
    }
}
