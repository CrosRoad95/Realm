using Realm.Domain.Registries;

namespace Realm.Console.Commands;

[CommandName("takeitem")]
public sealed class TakeItemCommand : IIngameCommand
{
    private readonly ILogger<TakeItemCommand> _logger;
    private readonly ItemsRegistry _itemsRegistry;

    public TakeItemCommand(ILogger<TakeItemCommand> logger, ItemsRegistry itemsRegistry)
    {
        _logger = logger;
        _itemsRegistry = itemsRegistry;
    }

    public Task Handle(Guid traceId, Entity entity, string[] args)
    {
        if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
        {
            uint itemId = uint.Parse(args.ElementAtOrDefault(0) ?? "1");
            uint count = uint.Parse(args.ElementAtOrDefault(1) ?? "1");
            inventoryComponent.RemoveItem(itemId);
            entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Item removed, {inventoryComponent.Number}/{inventoryComponent.Size}");
        }

        return Task.CompletedTask;
    }
}
