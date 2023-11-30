namespace RealmCore.Server.Interfaces.Vehicles;

public interface IVehicleFuelService : IVehicleService, IEnumerable<FuelContainer>
{
    FuelContainer AddFuelContainer(short fuelType, float initialAmount, float maxCapacity, float fuelConsumptionPerOneKm, float minimumDistanceThreshold, bool makeActive = false);
    void Update(bool forceUpdate = false);
}
