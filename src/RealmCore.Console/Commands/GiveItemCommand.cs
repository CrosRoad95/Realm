﻿namespace RealmCore.Console.Commands;

[CommandName("giveitem")]
public sealed class GiveItemCommand : IIngameCommand
{
    private readonly ILogger<GiveItemCommand> _logger;
    private readonly ItemsRegistry _itemsRegistry;

    public GiveItemCommand(ILogger<GiveItemCommand> logger, ItemsRegistry itemsRegistry)
    {
        _logger = logger;
        _itemsRegistry = itemsRegistry;
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
            entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Item added, {inventoryComponent.Number}/{inventoryComponent.Size}");
        }

        return Task.CompletedTask;
    }
}