using RealmCore.Server.Extensions;
using SlipeServer.Server.Services;

namespace RealmCore.Console.Commands;

[CommandName("giveitem")]
public sealed class GiveItemCommand : IIngameCommand
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

    public Task Handle(Entity entity, string[] args)
    {
        if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
        {
            uint itemId = uint.Parse(args.ElementAtOrDefault(0) ?? "1");
            uint count = uint.Parse(args.ElementAtOrDefault(1) ?? "1");
            inventoryComponent.AddItem(_itemsRegistry, itemId, count, new Dictionary<string, object>
            {
                ["foo"] = 10
            });
            _chatBox.OutputTo(entity, $"Item added, {inventoryComponent.Number}/{inventoryComponent.Size}");
        }

        return Task.CompletedTask;
    }
}
