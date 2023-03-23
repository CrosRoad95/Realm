using Realm.Persistance.DTOs;
using Realm.Persistance.Interfaces;

namespace Realm.Server.Services;

internal sealed class VehiclesService : IVehiclesService
{
    private readonly IVehicleRepository _vehicleRepository;

    public VehiclesService(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Entity> ConvertToPrivateVehicle(Entity vehicleEntity)
    {
        if (vehicleEntity.Tag != Entity.EntityTag.Vehicle)
            throw new InvalidOperationException();

        if (vehicleEntity.HasComponent<PrivateVehicleComponent>())
            throw new InvalidOperationException();

        var vehicleElementComponent = vehicleEntity.GetRequiredComponent<VehicleElementComponent>();
        await vehicleEntity.AddComponentAsync(new PrivateVehicleComponent(await _vehicleRepository.CreateNewVehicle(vehicleElementComponent.Model)));
        return vehicleEntity;
    }

    public Task<List<LightInfoVehicleDTO>> GetLightVehiclesByUserId(int userId)
    {
        return _vehicleRepository.GetLightVehiclesByUserId(userId);
    }

    public Task<List<Persistance.Data.Vehicle>> GetVehiclesByUserId(int userId)
    {
        return _vehicleRepository.GetVehiclesByUserId(userId);
    }

    public Task<List<Persistance.Data.Vehicle>> GetAllSpawnedVehicles()
    {
        return _vehicleRepository.GetAllSpawnedVehicles();
    }
}
