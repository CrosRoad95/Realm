using VehicleData = Realm.Persistance.Data.Vehicle;

namespace Realm.Domain.Components.Vehicles;

public class PrivateVehicleComponent : Component
{
    private readonly VehicleData _vehicleData;

    public VehicleData VehicleData => _vehicleData;
    public PrivateVehicleComponent(VehicleData vehicleData)
    {
        _vehicleData = vehicleData;
    }
}
