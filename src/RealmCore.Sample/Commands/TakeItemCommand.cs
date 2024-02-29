namespace RealmCore.Sample.Commands;

[CommandName("takeitem")]
public sealed class TakeItemCommand : IInGameCommand
{
    private readonly ILogger<TakeItemCommand> _logger;
    private readonly ChatBox _chatBox;

    public string[] RequiredPolicies { get; } = [];
    public TakeItemCommand(ILogger<TakeItemCommand> logger, ChatBox chatBox)
    {
        _logger = logger;
        _chatBox = chatBox;
    }

    public Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        if (player.Inventory.TryGetPrimary(out var inventory))
        {
            uint itemId = args.ReadUInt();
            uint count = args.ReadUInt();
            inventory.RemoveItem(itemId);
            _chatBox.OutputTo(player, $"Item removed, {inventory.Number}/{inventory.Size}");
        }

        return Task.CompletedTask;
    }
}
