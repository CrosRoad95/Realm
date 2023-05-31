using RealmCore.Server.Extensions;
using SlipeServer.Server.Services;

namespace RealmCore.Console.Commands;

[CommandName("takeitem")]
public sealed class TakeItemCommand : IIngameCommand
{
    private readonly ILogger<TakeItemCommand> _logger;
    private readonly ItemsRegistry _itemsRegistry;
    private readonly ChatBox _chatBox;

    public TakeItemCommand(ILogger<TakeItemCommand> logger, ItemsRegistry itemsRegistry, ChatBox chatBox)
    {
        _logger = logger;
        _itemsRegistry = itemsRegistry;
        _chatBox = chatBox;
    }

    public Task Handle(Entity entity, string[] args)
    {
        if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
        {
            uint itemId = uint.Parse(args.ElementAtOrDefault(0) ?? "1");
            uint count = uint.Parse(args.ElementAtOrDefault(1) ?? "1");
            inventoryComponent.RemoveItem(itemId);
            _chatBox.OutputTo(entity, $"Item removed, {inventoryComponent.Number}/{inventoryComponent.Size}");
        }

        return Task.CompletedTask;
    }
}
