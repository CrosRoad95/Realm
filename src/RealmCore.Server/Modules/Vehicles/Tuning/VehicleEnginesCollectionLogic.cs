namespace RealmCore.Server.Modules.Vehicles.Tuning;

internal class VehicleEnginesCollectionLogic
{
    public VehicleEnginesCollectionLogic(VehicleEnginesCollection vehicleEnginesCollection)
    {
        vehicleEnginesCollection.Add(1, new VehicleEngineCollectionItem
        {
            UpgradeId = 1,
        });
    }
}
