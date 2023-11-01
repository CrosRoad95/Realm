namespace RealmCore.Sample.Commands;

[CommandName("inventory")]
public sealed class InventoryCommand : IInGameCommand
{
    private readonly ILogger<InventoryCommand> _logger;
    private readonly ChatBox _chatBox;

    public InventoryCommand(ILogger<InventoryCommand> logger, ChatBox chatBox)
    {
        _logger = logger;
        _chatBox = chatBox;
    }

    public Task Handle(RealmPlayer player, CommandArguments args)
    {
        if (player.TryGetComponent(out InventoryComponent inventoryComponent))
        {
            _chatBox.OutputTo(player, $"Inventory, {inventoryComponent.Number}/{inventoryComponent.Size}");
            foreach (var item in inventoryComponent.Items)
            {
                _chatBox.OutputTo(player, $"Item, {item.ItemId} = {item.Name}");
            }

        }

        return Task.CompletedTask;
    }
}
