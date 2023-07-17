namespace RealmCore.Console.Commands;

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

    public Task Handle(Entity entity, CommandArguments args)
    {
        if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            _chatBox.OutputTo(entity, $"Inventory, {inventoryComponent.Number}/{inventoryComponent.Size}");
            foreach (var item in inventoryComponent.Items)
            {
                _chatBox.OutputTo(entity, $"Item, {item.ItemId} = {item.Name}");
            }

        }

        return Task.CompletedTask;
    }
}
