namespace RealmCore.Tests.Classes;

internal class TestItemsCollection : ItemsCollection
{
    public TestItemsCollection()
    {
        Add(1, new ItemsCollectionItem
        {
            AvailableActions = ItemAction.Use,
            Name = "Test item id 1",
            Size = 1,
            StackSize = 1,
        });
    }
}
