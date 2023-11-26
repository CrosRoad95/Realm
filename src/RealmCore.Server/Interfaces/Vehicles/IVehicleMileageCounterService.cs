
namespace RealmCore.Server.Interfaces.Vehicles;

public interface IVehicleMileageCounterService : IVehicleService
{
    float Mileage { get; set; }
    float MinimumDistanceThreshold { get; set; }

    event Action<IVehicleMileageCounterService, float, float>? Traveled;
}
