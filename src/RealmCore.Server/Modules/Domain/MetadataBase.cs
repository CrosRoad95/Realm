namespace RealmCore.Server.Modules.Domain;

public abstract class MetadataBase : Dictionary<string, object>
{
    public MetadataBase(ItemMetadata metadata) : base(metadata)
    {

    }

    public MetadataBase() { }
}
