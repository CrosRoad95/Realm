namespace RealmCore.Console.Commands;

[CommandName("licenses")]
public sealed class LicensesCommand : IIngameCommand
{
    private readonly ILogger<LicensesCommand> _logger;

    public LicensesCommand(ILogger<LicensesCommand> logger)
    {
        _logger = logger;
    }

    public Task Handle(Entity entity, string[] args)
    {
        if (entity.TryGetComponent(out LicensesComponent licenseComponent))
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.SendChatMessage($"Licenses");
            foreach (var license in licenseComponent.Licenses)
            {
                playerElementComponent.SendChatMessage($"License: {license.licenseId} = {license.IsSuspended}");
            }

        }
        return Task.CompletedTask;
    }
}
