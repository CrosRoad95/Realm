using Realm.Domain.Data;
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

    public async Task<List<VehicleLightInfo>> GetAllVehiclesLightInfoByOwnerId(int userId)
    {
        return await _vehicleRepository.GetAll()
            .Include(x => x.VehicleAccesses)
            .Where(x => x.VehicleAccesses.Any(x => x.UserId == userId))
            .Select(x => new VehicleLightInfo
            {
                Id = x.Id,
                Model = x.Model,
                Position = x.TransformAndMotion.Position
            })
            .ToListAsync();
    }
}
