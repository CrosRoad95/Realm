using RealmCore.ECS;

namespace RealmCore.Console.Commands;

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

    public Task Handle(Entity entity, CommandArguments args)
    {
        if (entity.TryGetComponent(out LicensesComponent licenseComponent))
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            _chatBox.OutputTo(entity, $"Licenses");
            foreach (var license in licenseComponent.Licenses)
            {
                _chatBox.OutputTo(entity, $"License: {license.licenseId} = {license.IsSuspended}");
            }

        }
        return Task.CompletedTask;
    }
}
