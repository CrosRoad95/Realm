namespace RealmCore.Server.Modules.Pickups;

public class RealmPickup : Pickup, IElementName
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public string? ElementName { get; set; }

    public RealmPickup(Vector3 position, ushort model) : base(position, model)
    {
    }
}
