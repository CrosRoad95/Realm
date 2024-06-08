namespace RealmCore.BlazorGui.Commands;

[CommandName("givelicense")]
public sealed class GiveLicenseCommand : IInGameCommand
{
    private readonly ILogger<GiveLicenseCommand> _logger;
    private readonly ChatBox _chatBox;

    public string[] RequiredPolicies { get; } = [];
    public GiveLicenseCommand(ILogger<GiveLicenseCommand> logger, ChatBox chatBox)
    {
        _logger = logger;
        _chatBox = chatBox;
    }

    public Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        var license = args.ReadInt();
        if (player.Licenses.TryAdd(license))
        {
            _chatBox.OutputTo(player, $"license added: '{license}'");
        }
        else
        {
            _chatBox.OutputTo(player, $"failed to add license: '{license}'");
        }
        return Task.CompletedTask;
    }
}
