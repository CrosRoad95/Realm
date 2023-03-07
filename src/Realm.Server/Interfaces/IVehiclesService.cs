using Realm.Persistance.DTOs;

namespace Realm.Server.Interfaces;

public interface IVehiclesService
{
    Task<Entity> ConvertToPrivateVehicle(Entity vehicleEntity);
    Task<List<VehicleModelPositionDTO>> GetAllVehiclesModelPositionDTOsByUserId(int userId);
}
