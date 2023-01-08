namespace Realm.Persistance.Data;

public class VehicleUpgrade
{
    public string VehicleId { get; set; }
    public uint UpgradeId { get; set; }

    public virtual Vehicle Vehicle { get; set; }
}
