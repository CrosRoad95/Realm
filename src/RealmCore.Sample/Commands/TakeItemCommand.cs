﻿using RealmCore.ECS;

namespace RealmCore.Console.Commands;

[CommandName("takeitem")]
public sealed class TakeItemCommand : IInGameCommand
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

    public Task Handle(Entity entity, CommandArguments args)
    {
        if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
        {
            uint itemId = args.ReadUInt();
            uint count = args.ReadUInt();
            inventoryComponent.RemoveItem(itemId);
            _chatBox.OutputTo(entity, $"Item removed, {inventoryComponent.Number}/{inventoryComponent.Size}");
        }

        return Task.CompletedTask;
    }
}