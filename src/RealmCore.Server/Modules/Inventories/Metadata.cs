namespace RealmCore.Server.Modules.Inventories;

public sealed class Metadata : Dictionary<string, object>
{
    public Metadata(Metadata metadata) : base(metadata)
    {

    }

    public Metadata() { }
}
