using Realm.Domain.Inventory;
using static Realm.Domain.Registries.ItemRegistryEntry;

namespace Realm.Console.Logic;

public class ItemsLogic
{
    public ItemsLogic(ItemsRegistry itemsRegistry)
    {
        itemsRegistry.UseCallback = Use;
        itemsRegistry.AddItem(1, new ItemRegistryEntry
        {
            Size = 1,
            StackSize = 4,
            Name = "Test item id 1",
            AvailiableActions = ItemAction.Use,
        });
        itemsRegistry.AddItem(2, new ItemRegistryEntry
        {
            Size = 2,
            StackSize = 4,
            Name = "Foo item id 2",
            AvailiableActions = ItemAction.Use,
        });
    }

    private Task Use(InventoryComponent inventoryComponent, Item item, ItemAction action)
    {
        switch(action)
        {
            case ItemAction.Use:
                inventoryComponent.Entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Item used: {item.Name}");
                inventoryComponent.RemoveItem(item);
                break;
        }
        return Task.CompletedTask;
    }
}
