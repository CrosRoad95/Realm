namespace RealmCore.Server.Security.Interfaces;

public interface IActiveVehicles
{
    IEnumerable<int> ActiveVehiclesIds { get; }

    bool IsActive(int vehicleId);
    bool TryGetVehicleById(int vehicleId, out RealmVehicle? vehicle);
    bool TrySetActive(int vehicleId, RealmVehicle vehicle);
    bool TrySetInactive(int vehicleId);
}
