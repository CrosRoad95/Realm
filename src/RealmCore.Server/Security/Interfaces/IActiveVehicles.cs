namespace RealmCore.Server.Security.Interfaces;

public interface IActiveVehicles
{
    IEnumerable<int> ActiveVehiclesIds { get; }

    bool IsActive(int vehicleId);
    bool TryGetEntityByVehicleId(int vehicleId, out Entity? entity);
    bool TrySetActive(int vehicleId, Entity entity);
    bool TrySetInactive(int vehicleId);
}
