namespace RealmCore.Server.Exceptions;

public class VehicleRemovedException : Exception
{
    public int VehicleId { get; }

    public VehicleRemovedException(int vehicleId)
    {
        VehicleId = vehicleId;
    }
}
