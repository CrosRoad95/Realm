namespace RealmCore.Server.Security;

internal sealed class ActiveVehicles : IActiveVehicles
{
    private readonly ConcurrentDictionary<int, Entity> _activeVehicles = new();

    public IEnumerable<int> ActiveVehiclesIds => _activeVehicles.Keys;
    public event Action<int, Entity>? Activated;
    public event Action<int, Entity>? Deactivated;

    public bool IsActive(int vehicleId)
    {
        return _activeVehicles.ContainsKey(vehicleId);
    }

    public bool TrySetActive(int vehicleId, Entity entity)
    {
        if (_activeVehicles.TryAdd(vehicleId, entity))
        {
            Activated?.Invoke(vehicleId, entity);
            return true;
        }
        return false;
    }

    public bool TrySetInactive(int vehicleId)
    {
        if (_activeVehicles.TryRemove(vehicleId, out var entity))
        {
            Deactivated?.Invoke(vehicleId, entity);
            return true;
        }
        return false;
    }

    public bool TryGetEntityByVehicleId(int userId, out Entity? entity) => _activeVehicles.TryGetValue(userId, out entity);
}
