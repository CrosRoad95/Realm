namespace RealmCore.Server.Exceptions;

public class PersistantVehicleNotFoundException : Exception
{
    public PersistantVehicleNotFoundException(string message) : base(message) { }
    public PersistantVehicleNotFoundException() { }
}
