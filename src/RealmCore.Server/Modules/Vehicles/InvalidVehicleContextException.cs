namespace RealmCore.Server.Modules.Vehicles;

public class InvalidVehicleContextException : Exception
{
    public InvalidVehicleContextException() : base("VehicleContext has no vehicle set") { }
}
