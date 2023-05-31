using RealmCore.Server.Extensions;
using SlipeServer.Server.Services;

namespace RealmCore.Console.Commands;

[CommandName("givelicense")]
public sealed class GiveLicenseCommand : IIngameCommand
{
    private readonly ILogger<GiveLicenseCommand> _logger;
    private readonly ChatBox _chatBox;

    public GiveLicenseCommand(ILogger<GiveLicenseCommand> logger, ChatBox chatBox)
    {
        _logger = logger;
        _chatBox = chatBox;
    }

    public Task Handle(Entity entity, string[] args)
    {
        if (entity.TryGetComponent(out LicensesComponent licenseComponent))
        {
            var license = args.First();
            if (licenseComponent.AddLicense(int.Parse(license)))
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
