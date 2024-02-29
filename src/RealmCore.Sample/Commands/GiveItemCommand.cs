namespace RealmCore.Sample.Commands;

[CommandName("giveitem")]
public sealed class GiveItemCommand : IInGameCommand
{
    private readonly ILogger<GiveItemCommand> _logger;
    private readonly ItemsCollection _itemsCollection;
    private readonly ChatBox _chatBox;

    public string[] RequiredPolicies { get; } = [];
    public GiveItemCommand(ILogger<GiveItemCommand> logger, ItemsCollection itemsCollection, ChatBox chatBox)
    {
        _logger = logger;
        _itemsCollection = itemsCollection;
        _chatBox = chatBox;
    }

    public Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        if (player.Inventory.TryGetPrimary(out var inventory))
        {
            uint itemId = args.ReadUInt();
            uint count = args.ReadUInt();
            inventory.AddItem(_itemsCollection, itemId, count, new Metadata
            {
                ["foo"] = 10
            });
            _chatBox.OutputTo(player, $"Item added, {inventory.Number}/{inventory.Size}");
        }

        return Task.CompletedTask;
    }
}
