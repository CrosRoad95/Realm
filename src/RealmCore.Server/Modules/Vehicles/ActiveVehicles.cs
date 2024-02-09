namespace RealmCore.Server.Modules.Vehicles;

public interface IActiveVehicles
{
    IEnumerable<int> ActiveVehiclesIds { get; }

    bool IsActive(int vehicleId);
    bool TryGetVehicleById(int vehicleId, out RealmVehicle? vehicle);
    bool TrySetActive(int vehicleId, RealmVehicle vehicle);
    bool TrySetInactive(int vehicleId);
}

internal sealed class ActiveVehicles : IActiveVehicles
{
    private readonly ConcurrentDictionary<int, RealmVehicle> _activeVehicles = new();

    public IEnumerable<int> ActiveVehiclesIds => _activeVehicles.Keys;
    public event Action<int, RealmVehicle>? Activated;
    public event Action<int, RealmVehicle>? Deactivated;

    public bool IsActive(int vehicleId)
    {
        return _activeVehicles.ContainsKey(vehicleId);
    }

    public bool TrySetActive(int vehicleId, RealmVehicle vehicle)
    {
        if (_activeVehicles.TryAdd(vehicleId, vehicle))
        {
            Activated?.Invoke(vehicleId, vehicle);
            return true;
        }
        return false;
    }

    public bool TrySetInactive(int vehicleId)
    {
        if (_activeVehicles.TryRemove(vehicleId, out var vehicle))
        {
            Deactivated?.Invoke(vehicleId, vehicle);
            return true;
        }
        return false;
    }

    public bool TryGetVehicleById(int userId, out RealmVehicle? vehicle) => _activeVehicles.TryGetValue(userId, out vehicle);
}
