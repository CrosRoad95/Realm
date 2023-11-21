

namespace RealmCore.Server.Interfaces.Vehicles;

public interface IVehiclePersistanceService : IVehicleService
{
    int Id { get; }
    byte Kind { get; }
    DateTime? LastUsed { get; }
    bool IsLoaded { get; }
    internal VehicleData VehicleData { get; }

    event Action<IVehiclePersistanceService, RealmVehicle>? Loaded;

    void Load(VehicleData vehicleData);
}
