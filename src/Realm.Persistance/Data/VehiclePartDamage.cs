namespace Realm.Persistance.Data;

public class VehiclePartDamage
{
    public int VehicleId { get; set; }
    public short PartId { get; set; }
    public float State { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
