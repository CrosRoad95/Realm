﻿using RealmCore.Server.Components;
using RealmCore.Server.Enums;
using RealmCore.Server.Inventory;
using SlipeServer.Server.Enums;

namespace RealmCore.Console.Logic;

public class ItemsLogic
{
    public ItemsLogic(ItemsRegistry itemsRegistry, IECS ecs)
    {
        itemsRegistry.UseCallback = Use;
        itemsRegistry.Add(1, new ItemRegistryEntry
        {
            Size = 1,
            StackSize = 4,
            Name = "Test item id 1",
            AvailiableActions = ItemAction.Use,
        });
        itemsRegistry.Add(2, new ItemRegistryEntry
        {
            Size = 2,
            StackSize = 4,
            Name = "Foo item id 2",
            AvailiableActions = ItemAction.Use,
        });
        itemsRegistry.Add(3, new ItemRegistryEntry
        {
            Size = 1,
            StackSize = 1,
            Name = "Sample weapon",
            AvailiableActions = ItemAction.None,
        });
        itemsRegistry.Add(4, new ItemRegistryEntry
        {
            Size = 0.05m,
            StackSize = 10,
            Name = "item size 0.05",
            AvailiableActions = ItemAction.None,
        });

        ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (entity.Tag == EntityTag.Player)
            entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is InventoryComponent inventoryComponent)
        {
            inventoryComponent.ItemAdded += HandleInventoryComponentItemAdded;
            inventoryComponent.ItemRemoved += HandleInventoryComponentItemRemoved;
        }
    }

    private void HandleInventoryComponentItemRemoved(InventoryComponent inventoryComponent, Item item)
    {
        var itemId = item.ItemId;
        switch (itemId)
        {
            case 3:
                if (!inventoryComponent.HasItemById(itemId))
                {
                    var playerElementComponent = inventoryComponent.Entity.GetRequiredComponent<PlayerElementComponent>();
                    playerElementComponent.GiveWeapon(WeaponId.Bat);
                    playerElementComponent.SendChatMessage("Bat taken");
                }
                break;
        }
    }

    private void HandleInventoryComponentItemAdded(InventoryComponent inventoryComponent, Item item)
    {
        var itemId = item.ItemId;
        switch (itemId)
        {
            case 3:
                var playerElementComponent = inventoryComponent.Entity.GetRequiredComponent<PlayerElementComponent>();
                if (!playerElementComponent.HasWeapon(WeaponId.Bat))
                {
                    playerElementComponent.GiveWeapon(WeaponId.Bat, 1);
                    playerElementComponent.SendChatMessage("Bat given");
                }
                break;
        }
    }

    private Task Use(InventoryComponent inventoryComponent, Item item, ItemAction action)
    {
        switch (action)
        {
            case ItemAction.Use:
                inventoryComponent.Entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Item used: {item.Name}");
                inventoryComponent.RemoveItem(item);
                break;
        }
        return Task.CompletedTask;
    }
}