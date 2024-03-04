namespace RealmCore.Server.Modules.Vehicles;

/// <summary>
/// Vehicle scoped set of methods
/// </summary>
public interface IVehicleService
{
    Task ConvertToPersistantVehicle(CancellationToken cancellationToken = default);
    Task<bool> SetVehicleSpawned(bool spawned = true, CancellationToken cancellationToken = default);
}

internal sealed class VehicleService : IVehicleService
{
    private readonly RealmVehicle _vehicle;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IElementFactory _elementFactory;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IVehiclesInUse _vehiclesInUse;

    public VehicleService(VehicleContext vehicleContext, IVehicleRepository vehicleRepository, IElementFactory elementFactory, IDateTimeProvider dateTimeProvider, IVehiclesInUse vehiclesInUse)
    {
        _vehicle = vehicleContext.Vehicle;
        _vehicleRepository = vehicleRepository;
        _elementFactory = elementFactory;
        _dateTimeProvider = dateTimeProvider;
        _vehiclesInUse = vehiclesInUse;
    }

    public async Task ConvertToPersistantVehicle(CancellationToken cancellationToken = default)
    {
        if (_vehicle.Persistence.IsLoaded)
            throw new InvalidOperationException();

        var vehicleData = await _vehicleRepository.CreateVehicle(_vehicle.Model, _dateTimeProvider.Now, cancellationToken);

        SetActive(vehicleData.Id, _vehicle);
        _vehicle.Persistence.Load(vehicleData);
    }

    public async Task<bool> SetVehicleSpawned(bool spawned = true, CancellationToken cancellationToken = default)
    {
        return await _vehicleRepository.SetSpawned(_vehicle.Persistence.Id, spawned, cancellationToken);
    }

    private void SetActive(int vehicleId, RealmVehicle vehicle)
    {
        if (!_vehiclesInUse.TrySetActive(vehicleId, vehicle))
            throw new PersistantVehicleAlreadySpawnedException("Failed to create already existing vehicle.");
    }

}
