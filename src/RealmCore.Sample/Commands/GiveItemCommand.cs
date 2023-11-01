namespace RealmCore.Sample.Commands;

[CommandName("giveitem")]
public sealed class GiveItemCommand : IInGameCommand
{
    private readonly ILogger<GiveItemCommand> _logger;
    private readonly ItemsRegistry _itemsRegistry;
    private readonly ChatBox _chatBox;

    public GiveItemCommand(ILogger<GiveItemCommand> logger, ItemsRegistry itemsRegistry, ChatBox chatBox)
    {
        _logger = logger;
        _itemsRegistry = itemsRegistry;
        _chatBox = chatBox;
    }

    public Task Handle(RealmPlayer player, CommandArguments args)
    {
        if (player.Components.TryGetComponent(out InventoryComponent inventoryComponent))
        {
            uint itemId = args.ReadUInt();
            uint count = args.ReadUInt();
            inventoryComponent.AddItem(_itemsRegistry, itemId, count, new Metadata
            {
                ["foo"] = 10
            });
            _chatBox.OutputTo(player, $"Item added, {inventoryComponent.Number}/{inventoryComponent.Size}");
        }

        return Task.CompletedTask;
    }
}
