namespace RealmCore.Server.Modules.Vehicles.Tuning;

public class VehicleEngineCollectionItem : CollectionItemBase
{
    public int UpgradeId { get; set; }
}

public class VehicleEnginesCollection : CollectionBase<VehicleEngineCollectionItem>
{
    public VehicleEnginesCollection()
    {
        Add(1, new VehicleEngineCollectionItem
        {
            UpgradeId = 1,
        });
    }
}
