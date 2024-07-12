namespace RealmCore.Server.Modules.Vehicles;

/// <summary>
/// Vehicle scoped set of methods
/// </summary>
public interface IVehicleService
{
    Task Destroy(CancellationToken cancellationToken = default);
    Task<bool> SetVehicleSpawned(bool spawned = true, CancellationToken cancellationToken = default);
}

internal sealed class VehicleService : IVehicleService
{
    private readonly RealmVehicle _vehicle;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehiclesInUse _vehiclesInUse;
    private readonly IElementSaveService _saveService;

    public VehicleService(VehicleContext vehicleContext, IVehicleRepository vehicleRepository, IDateTimeProvider dateTimeProvider, IVehiclesInUse vehiclesInUse, IElementSaveService saveService)
    {
        _vehicle = vehicleContext.Vehicle;
        _vehicleRepository = vehicleRepository;
        _vehiclesInUse = vehiclesInUse;
        _saveService = saveService;
    }

    public async Task<bool> SetVehicleSpawned(bool spawned = true, CancellationToken cancellationToken = default)
    {
        return await _vehicleRepository.SetSpawned(_vehicle.Persistence.Id, spawned, cancellationToken);
    }

    public async Task Destroy(CancellationToken cancellationToken = default)
    {
        if (_vehicle.Persistence.IsLoaded)
        {
            await _saveService.Save(cancellationToken);
            var id = _vehicle.Persistence.Id;
            await _vehicleRepository.SetSpawned(id, false, cancellationToken);
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
