namespace RealmCore.Server.Modules.Inventories;

public sealed class ItemMetadata : Dictionary<string, object>
{
    public ItemMetadata(ItemMetadata metadata) : base(metadata)
    {

    }

    public ItemMetadata() { }
}
