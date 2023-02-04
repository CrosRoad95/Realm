namespace Realm.Persistance.Data;

public class VehicleUpgrade
{
    public int VehicleId { get; set; }
    public int UpgradeId { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
