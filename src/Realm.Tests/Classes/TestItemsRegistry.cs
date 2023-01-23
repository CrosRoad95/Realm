using Realm.Domain.Registries;

namespace Realm.Tests.Classes;

internal class TestItemsRegistry : ItemsRegistry
{
    public TestItemsRegistry()
    {
        AddItem(1, new ItemRegistryEntry
        {
            AvailiableActions = ItemRegistryEntry.ItemAction.Use,
            Name = "Test item id 1",
            Size = 1,
            StackSize = 1,
        });
    }
}
