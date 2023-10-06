namespace RealmCore.Server.Concepts;

public sealed class Metadata : Dictionary<string, object>
{
    public Metadata(Metadata metadata) : base(metadata)
    {

    }

    public Metadata() { }
}
