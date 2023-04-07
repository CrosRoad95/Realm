namespace RealmCore.Console.Commands;

[CommandName("inventory")]
public sealed class InventoryCommand : IIngameCommand
{
    private readonly ILogger<InventoryCommand> _logger;

    public InventoryCommand(ILogger<InventoryCommand> logger)
    {
        _logger = logger;
    }

    public Task Handle(Entity entity, string[] args)
    {
        if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.SendChatMessage($"Inventory, {inventoryComponent.Number}/{inventoryComponent.Size}");
            foreach (var item in inventoryComponent.Items)
            {
                playerElementComponent.SendChatMessage($"Item, {item.ItemId} = {item.Name}");
            }

        }

        return Task.CompletedTask;
    }
}
