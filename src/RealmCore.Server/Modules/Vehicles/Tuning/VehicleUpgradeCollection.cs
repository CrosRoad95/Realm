namespace RealmCore.Server.Modules.Vehicles.Tuning;

public class VehicleUpgradesCollectionItem : CollectionItemBase
{
    public IVehicleHandlingModifier VehicleUpgrade { get; }

    public VehicleUpgradesCollectionItem(IVehicleHandlingModifier vehicleUpgrade)
    {
        VehicleUpgrade = vehicleUpgrade;
    }
}

public class VehicleUpgradeCollection : CollectionBase<VehicleUpgradesCollectionItem>
{

}
