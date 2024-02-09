namespace RealmCore.Server.Modules.Vehicles;

public class VehicleAccessDefinedException : Exception
{
    public VehicleAccessDefinedException() : base() { }
    public VehicleAccessDefinedException(string message) : base(message) { }
    public VehicleAccessDefinedException(string message, Exception innerException) : base(message, innerException) { }
}
