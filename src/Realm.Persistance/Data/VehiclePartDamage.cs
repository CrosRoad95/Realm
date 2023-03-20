namespace Realm.Persistance.Data;

public sealed class VehiclePartDamage
{
    public int VehicleId { get; set; }
    public short PartId { get; set; }
    public float State { get; set; }
}
