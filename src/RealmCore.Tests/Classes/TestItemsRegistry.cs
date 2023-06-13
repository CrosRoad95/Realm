namespace RealmCore.Tests.Classes;

internal class TestItemsRegistry : ItemsRegistry
{
    public TestItemsRegistry()
    {
        Add(1, new ItemRegistryEntry
        {
            AvailableActions = ItemAction.Use,
            Name = "Test item id 1",
            Size = 1,
            StackSize = 1,
        });
    }
}
