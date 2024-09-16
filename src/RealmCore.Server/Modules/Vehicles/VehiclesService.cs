using RealmCore.Persistence.Repository;
using SlipeServer.Server.Elements;
using System.Threading;
namespace RealmCore.Server.Modules.Vehicles;

public sealed class VehiclesService
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private readonly IElementFactory _elementFactory;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly UsersInUse _activeUsers;
    private readonly VehiclesInUse _vehiclesInUse;
    private readonly ILogger<VehiclesService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _scopedServiceProvider;
    private readonly VehiclesRepository _vehicleRepository;

    public VehiclesService(IElementFactory elementFactory, IDateTimeProvider dateTimeProvider, UsersInUse activeUsers, VehiclesInUse vehiclesInUse, ILogger<VehiclesService> logger, IServiceProvider serviceProvider)
    {
        _elementFactory = elementFactory;
        _dateTimeProvider = dateTimeProvider;
        _activeUsers = activeUsers;
        _vehiclesInUse = vehiclesInUse;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _serviceScope = _serviceProvider.CreateScope();
        _scopedServiceProvider = _serviceScope.ServiceProvider;
        _vehicleRepository = _scopedServiceProvider.GetRequiredService<VehiclesRepository>();
    }

    public async Task<RealmVehicle?> CreatePersistantVehicle(Location location, VehicleModel model, CancellationToken cancellationToken = default)
    {
        var vehicle = _elementFactory.CreateVehicle(location, model);
        var vehicleData = await vehicle.GetRequiredService<VehiclesRepository>().CreateVehicle((ushort)model, _dateTimeProvider.Now, cancellationToken);

        if (_vehiclesInUse.TrySetActive(vehicleData.Id, vehicle))
        {
            vehicle.Persistence.Load(vehicleData, true);
            return vehicle;
        }
        else
        {
            vehicle.Destroy();
            return null;
        }
    }

    public async Task<VehicleData[]> GetAllVehicles(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _vehicleRepository.GetVehiclesByUserId(player.UserId, null, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    
    public async Task<VehicleData[]> GetAllVehicles(int userId, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _vehicleRepository.GetVehiclesByUserId(userId, null, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task<LightInfoVehicleDto[]> GetAllLightVehicles(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _vehicleRepository.GetLightVehiclesByUserId(player.UserId, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    
    public async Task<LightInfoVehicleDto[]> GetAllLightVehicles(int userId, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _vehicleRepository.GetLightVehiclesByUserId(userId, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public IEnumerable<RealmPlayer> GetOnlineOwners(RealmVehicle vehicle)
    {
        foreach (var owner in vehicle.Access.Owners)
        {
            if (_activeUsers.TryGetPlayerByUserId(owner.UserId, out var player) && player != null)
                yield return player;
        }
    }

    public Task Destroy(RealmVehicle vehicle)
    {
        return vehicle.GetRequiredService<VehicleService>().Destroy();
    }

    public async Task<bool> Save(RealmVehicle vehicle)
    {
        if (!vehicle.Persistence.IsLoaded)
            return false;

        return await vehicle.GetRequiredService<ElementSaveService>().Save();
    }

    public async Task<RealmVehicle?> ConvertToPersistantVehicle(RealmVehicle vehicle, CancellationToken cancellationToken = default)
    {
        if (vehicle.Persistence.IsLoaded)
            throw new InvalidOperationException();

        var vehicleRepository = vehicle.GetRequiredService<VehiclesRepository>();
        var vehicleService = vehicle.GetRequiredService<VehicleService>();
        var location = vehicle.GetLocation();

        var vehicleData = await vehicleRepository.CreateVehicle(vehicle.Model, _dateTimeProvider.Now, cancellationToken);
        if (!_vehiclesInUse.TrySetActive(vehicleData.Id, vehicle))
        {
            return null;
        }

        vehicle.Persistence.Load(vehicleData, true);

        var occupants = vehicle.Occupants.ToArray();
        await vehicleService.Destroy(cancellationToken);

        using var scope = _serviceProvider.CreateScope();
        var vehicleLoader = scope.ServiceProvider.GetRequiredService<VehiclesLoader>();
        var persistentVehicle = await vehicleLoader.LoadVehicleById(vehicleData.Id, cancellationToken);
        persistentVehicle.SetLocation(location);
        foreach (var pair in occupants)
            persistentVehicle.AddPassenger(pair.Key, pair.Value);

        return persistentVehicle;
    }

    public async Task<bool> SetSetting(int vehicleId, int settingId, object? value, CancellationToken cancellationToken = default)
    {
        var stringValue = JsonHelpers.Serialize(value);
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _vehicleRepository.SetSetting(vehicleId, settingId, stringValue, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    
    public async Task<bool> SetSetting(int[] vehiclesIds, int settingId, object? value, CancellationToken cancellationToken = default)
    {
        var stringValue = JsonHelpers.Serialize(value);
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _vehicleRepository.SetSetting(vehiclesIds, settingId, stringValue, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    
    public async Task<bool> RemoveAllSettings(int vehicleId, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _vehicleRepository.RemoveAllSettings(vehicleId, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    
    public async Task<bool> RemoveAllSettings(int[] vehiclesIds, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _vehicleRepository.RemoveAllSettings(vehiclesIds, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task<string?> GetSetting(int vehicleId, int settingId, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _vehicleRepository.GetSetting(vehicleId, settingId, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

}
