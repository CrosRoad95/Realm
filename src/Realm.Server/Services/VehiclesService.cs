using Realm.Persistance.Interfaces;

namespace Realm.Server.Services;

public sealed class VehiclesService
{
    private readonly IVehicleRepository _vehicleRepository;

    internal VehiclesService(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Entity> ConvertToPrivateVehicle(Entity vehicleEntity)
    {
        if (vehicleEntity.Tag != Entity.VehicleTag)
            throw new InvalidOperationException();

        if (vehicleEntity.HasComponent<PrivateVehicleComponent>())
            return vehicleEntity;

        vehicleEntity.AddComponent(new PrivateVehicleComponent(await _vehicleRepository.CreateNewVehicle()));
        return vehicleEntity;
    }
}
