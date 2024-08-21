namespace RealmCore.Server.Modules.Vehicles;

/// <summary>
/// Vehicle scoped set of methods
/// </summary>
public sealed class VehicleService
{
    private readonly RealmVehicle _vehicle;
    private readonly VehicleRepository _vehicleRepository;
    private readonly VehiclesInUse _vehiclesInUse;
    private readonly ElementSaveService _saveService;

    public VehicleService(VehicleContext vehicleContext, VehicleRepository vehicleRepository, IDateTimeProvider dateTimeProvider, VehiclesInUse vehiclesInUse, ElementSaveService saveService)
    {
        _vehicle = vehicleContext.Vehicle;
        _vehicleRepository = vehicleRepository;
        _vehiclesInUse = vehiclesInUse;
        _saveService = saveService;
    }

    public async Task<bool> SetVehicleSpawned(bool spawned = true, CancellationToken cancellationToken = default)
    {
        return await _vehicleRepository.TrySetSpawned(_vehicle.Persistence.Id, spawned, cancellationToken);
    }

    public async Task Destroy(CancellationToken cancellationToken = default)
    {
        if (_vehicle.Persistence.IsLoaded)
        {
            await _saveService.Save(cancellationToken);
            var id = _vehicle.Persistence.Id;
            await _vehicleRepository.TrySetSpawned(id, false, cancellationToken);
            _vehiclesInUse.TrySetInactive(id);
            _vehicle.Persistence.Unload();
        }

        foreach (var occupants in _vehicle.Occupants.Values)
        {
            occupants.RemoveFromVehicle();
        }
        await _vehicle.DisposeAsync();
    }
}
