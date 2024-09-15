namespace RealmCore.Server.Modules.Vehicles.Settings;

public sealed class VehicleSettingsFeature : ElementSettingsFeature<VehicleSettingData>, IVehicleFeature, IUsesVehiclePersistentData
{
    public RealmVehicle Vehicle { get; init; }
    public VehicleSettingsFeature(VehicleContext vehicleContext)
    {
        Vehicle = vehicleContext.Vehicle;
    }

    public void Loaded(VehicleData vehicleData, bool preserveData = false)
    {
        Load(vehicleData.Settings);
    }

    public void Unloaded()
    {

    }
}
